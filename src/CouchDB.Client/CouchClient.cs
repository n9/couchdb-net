﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using CouchDB.Client.Responses;
using Flurl;
using Flurl.Http;

namespace CouchDB.Client
{
    public class CouchClient
    {
        private class AuthorizationData
        {
            public bool NeedAuthentication { get; set; }
            public string AuthName { get; set; }
            public string AuthPassword { get; set; }
            public string AuthToken { get; set; }
            public DateTime AuthTokenDate { get; set; }
            public int AuthTokenDuration { get; set; }
        }

        private readonly AuthorizationData _authData;
        private readonly string _serverUrl;

        internal IFlurlRequest BaseRequest
        {
            get
            {
                if (_authData.NeedAuthentication && (_authData.AuthToken == null || _authData.AuthTokenDate.AddMinutes(_authData.AuthTokenDuration) >= DateTime.Now))
                    Login().Wait();
        
                var request = _serverUrl.EnableCookies();
                return _authData.NeedAuthentication ? 
                    request.WithCookie("AuthSession", _authData.AuthToken) : 
                    request;
            }
        }
        
        public CouchClient(string serverUrl)
        {
            _serverUrl = serverUrl;
            _authData = new AuthorizationData();
        }

        #region Authentication
        
        public void ConfigureAuthentication(string name, string password, int tokenDurationMinutes = 10)
        {
            _authData.NeedAuthentication = true;
            _authData.AuthName = name;
            _authData.AuthPassword = password;
            _authData.AuthTokenDuration = tokenDurationMinutes;
        }

        private async Task Login()
        {
            var response = await _serverUrl
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _authData.AuthName,
                    password = _authData.AuthPassword
                });

            _authData.AuthTokenDate = DateTime.Now;

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var dirtyToken = values.First();
                var regex = new Regex(@"^AuthSession=(.+); Version=1; Path=/; HttpOnly$");
                var match = regex.Match(dirtyToken);
                if (match.Success)
                {
                    _authData.AuthToken = match.Groups[1].Value;
                    return;
                }
            }

            throw new InvalidOperationException("Error while trying to log-in.");
        }

        #endregion

        #region Database operation

        public async Task<DbInfo> GetDatabaseInfoAsync(string dbName)
        {
            var request = BaseRequest
                .AppendPathSegment(dbName)
                .GetJsonAsync<DbInfo>();

            return await RequestsHelper.SendAsync(request);
        }

        public async Task<CouchDatabase<T>> GetDatabaseAsync<T>(string dbName) where T : CouchEntity
        {
            var request = GetDatabaseInfoAsync(dbName);
            var info = await RequestsHelper.SendAsync(request);

            return new CouchDatabase<T>(info.DbName, BaseRequest);
        }

        public async Task<IEnumerable<string>> GetDatabasesNamesAsync()
        {
            var request = BaseRequest
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>();

            return await RequestsHelper.SendAsync(request);
        }

        public async Task AddDatabaseAsync(string dbName)
        {
            var request = BaseRequest
                .AppendPathSegment(dbName)
                .PutAsync(null);

            await RequestsHelper.SendAsync(request);
        }

        public async Task RemoveDatabaseAsync(string dbName)
        {
            var request = BaseRequest
                .AppendPathSegment(dbName)
                .DeleteAsync();

            await RequestsHelper.SendAsync(request);
        }

        #endregion
    }
}