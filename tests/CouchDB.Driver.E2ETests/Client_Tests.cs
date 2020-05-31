using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using Newtonsoft.Json;
using Xunit;

namespace CouchDB.Driver.E2E
{
    [Trait("Category", "Integration")]
    public class ClientTests
    {
        [Fact]
        public async Task Crud()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
                }

                rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

                Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 }).ConfigureAwait(false);
                Assert.Equal("Luke", luke.Name);

                luke.Surname = "Skywalker";
                luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
                Assert.Equal("Skywalker", luke.Surname);

                luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Equal(19, luke.Age);

                await rebels.DeleteAsync(luke).ConfigureAwait(false);
                luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Null(luke);

                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Crud_SpecialCharacters()
        {
            var databaseName = "rebel0_$()+/-";

            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>(databaseName);

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>(databaseName).ConfigureAwait(false);
                }

                rebels = await client.CreateDatabaseAsync<Rebel>(databaseName).ConfigureAwait(false);

                Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 }).ConfigureAwait(false);
                Assert.Equal("Luke", luke.Name);

                luke.Surname = "Skywalker";
                luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
                Assert.Equal("Skywalker", luke.Surname);

                luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Equal(19, luke.Age);

                await rebels.DeleteAsync(luke).ConfigureAwait(false);
                luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Null(luke);

                await client.DeleteDatabaseAsync<Rebel>(databaseName).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Users()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<CouchUser> users = client.GetUsersDatabase();

                if (!dbs.Contains(users.Database))
                {
                    users = await client.CreateDatabaseAsync<CouchUser>().ConfigureAwait(false);
                }

                CouchUser luke = await users.CreateAsync(new CouchUser(name: "luke", password: "lasersword")).ConfigureAwait(false);
                Assert.Equal("luke", luke.Name);

                luke = await users.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Equal("luke", luke.Name);

                await users.DeleteAsync(luke).ConfigureAwait(false);
                luke = await users.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Null(luke);

                await client.DeleteDatabaseAsync<CouchUser>().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Attachment()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
                }

                rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

                var luke = new Rebel { Name = "Luke", Age = 19 };
                var runningPath = Directory.GetCurrentDirectory();

                // Create
                luke.Attachments.AddOrUpdate($@"{runningPath}\Assets\luke.txt", MediaTypeNames.Text.Plain);
                luke = await rebels.CreateAsync(luke).ConfigureAwait(false);
                
                Assert.Equal("Luke", luke.Name);
                Assert.NotEmpty(luke.Attachments);

                CouchAttachment attachment = luke.Attachments.First();
                Assert.NotNull(attachment);
                Assert.NotNull(attachment.Uri);

                // Download
                var downloadFilePath = await rebels.DownloadAttachment(attachment, $@"{runningPath}\Assets", "luke-downloaded.txt");

                Assert.True(File.Exists(downloadFilePath));
                File.Delete(downloadFilePath);

                // Find
                luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
                Assert.Equal(19, luke.Age);
                attachment = luke.Attachments.First();
                Assert.NotNull(attachment);
                Assert.NotNull(attachment.Uri);
                Assert.NotNull(attachment.Digest);
                Assert.NotNull(attachment.Length);

                // Update
                luke.Surname = "Skywalker";
                luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
                Assert.Equal("Skywalker", luke.Surname);

                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Changes()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
                }

                rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

                Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 }).ConfigureAwait(false);
                Assert.Equal("Luke", luke.Name);

                var options = new ChangesFeedOptions
                {
                    IncludeDocs = true
                };
                var filter = ChangesFeedFilter.Selector<Rebel>(r => r.Name == "Luke" && r.Age == 19);
                var changesResult = await rebels.GetChangesAsync(options, filter);
                Assert.NotEmpty(changesResult.Results);
                Assert.Equal(changesResult.Results[0].Id, luke.Id);

                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ContinuousChanges()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
                CouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

                if (dbs.Contains(rebels.Database))
                {
                    await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
                }

                rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

                Rebel luke = await rebels.CreateAsync(new Rebel {Name = "Luke", Age = 19}).ConfigureAwait(false);
                Assert.Equal("Luke", luke.Name);


                luke.Surname = "Vader";
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    await rebels.CreateOrUpdateAsync(luke);
                });

                var ids = new[] {luke.Id};
                var option = new ChangesFeedOptions
                {
                    Since = "now"
                };
                var filter = ChangesFeedFilter.DocumentIds(ids);
                using var cancelSource = new CancellationTokenSource();
                await foreach (var _ in rebels.GetContinuousChangesAsync(option, filter, cancelSource.Token))
                {
                    cancelSource.Cancel();
                }

                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }
        }
    }
}