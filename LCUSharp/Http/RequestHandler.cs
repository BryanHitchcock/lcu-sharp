﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LCUSharp.Http
{
    /// <summary>
    /// A request handler that supports authentication.
    /// </summary>
    public abstract class RequestHandler
    {
        /// <summary>
        /// The HttpClient used to make requests.
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="RequestHandler"/> class.
        /// </summary>
        public RequestHandler()
        {
            var httpHandler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };
            httpHandler.ServerCertificateCustomValidationCallback = (response, cert, chain, errors) => true;
            HttpClient = new HttpClient(httpHandler);
        }

        /// <summary>
        /// Prepares a request by calculating the proper url.
        /// </summary>
        /// <typeparam name="TRequest">The object to serialize into the body.</typeparam>
        /// <param name="relativeUrl">The relative url.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="body">The request's body.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns></returns>
        protected async Task<HttpRequestMessage> PrepareRequestAsync<TRequest>(HttpMethod httpMethod, string relativeUrl, TRequest body, IEnumerable<string> queryParameters)
        {
            var url = queryParameters == null
                ? relativeUrl
                : relativeUrl + BuildQueryParameterString(queryParameters);
            var request = new HttpRequestMessage(httpMethod, url);

            if (body != null)
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(body)).ConfigureAwait(false);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }

        /// <summary>
        /// Builds the query parameter string that's appended to the request url.
        /// </summary>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns>The query parameter url string.</returns>
        protected string BuildQueryParameterString(IEnumerable<string> queryParameters)
        {
            return "?" + string.Join("&", queryParameters.Where(a => !string.IsNullOrWhiteSpace(a)));
        }

        /// <summary>
        /// Gets a <see cref="HttpResponseMessage"/>'s content.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>'s content.</returns>
        protected async Task<string> GetResponseContentAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}