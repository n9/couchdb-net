﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.LocalDocuments
{
    /// <summary>
    /// Perform operations on local (non-replicating) documents.
    /// </summary>
    public interface ILocalDocuments
    {
        /// <summary>
        /// Return all local documents.
        /// </summary>
        /// <param name="options">Options to apply to the query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains basic info about the documents.
        /// </returns>
        Task<IList<LocalCouchDocument>> GetAsync(LocalDocumentsOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return all documents with the supplied keys.
        /// </summary>
        /// <param name="keys">The IDs to use as filter.</param>
        /// <param name="options">Options to apply to the query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains basic info about the documents.
        /// </returns>
        Task<IList<LocalCouchDocument>> GetAsync(IReadOnlyCollection<string> keys, LocalDocumentsOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the specified local document. 
        /// </summary>
        /// <typeparam name="TSource">The type of the document.</typeparam>
        /// <param name="id">The ID of the document.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the document content.
        /// </returns>
        Task<TSource> GetAsync<TSource>(string id, CancellationToken cancellationToken = default)
            where TSource: LocalCouchDocument;

        /// <summary>
        /// Stores the specified local document. 
        /// </summary>
        /// <typeparam name="TSource">The type of the document.</typeparam>
        /// <param name="document"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task AddAsync<TSource>(TSource document, CancellationToken cancellationToken = default)
            where TSource : LocalCouchDocument;

        /// <summary>
        /// Deletes the specified local document. 
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
