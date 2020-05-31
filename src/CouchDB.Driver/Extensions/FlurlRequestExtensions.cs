using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Content;

namespace CouchDB.Driver.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "<Pending>")]
    public static class FlurlRequestExtensions
    {
        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="request">The IFlurlRequest instance.</param>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public static Task<Stream> PostJsonStreamAsync(
            this IFlurlRequest request,
            object data,
            CancellationToken cancellationToken = default(CancellationToken),
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            CapturedJsonContent capturedJsonContent = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
            return request.SendAsync(HttpMethod.Post, (HttpContent)capturedJsonContent, cancellationToken, completionOption).ReceiveStream();
        }


        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="request">The IFlurlRequest instance.</param>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public static Task<Stream> PostStringStreamAsync(
            this IFlurlRequest request,
            string data,
            CancellationToken cancellationToken = default(CancellationToken),
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            CapturedStringContent capturedStringContent = new CapturedStringContent(data, (Encoding)null, (string)null);
            return request.SendAsync(HttpMethod.Post, (HttpContent)capturedStringContent, cancellationToken, completionOption).ReceiveStream();
        }
    }
}
