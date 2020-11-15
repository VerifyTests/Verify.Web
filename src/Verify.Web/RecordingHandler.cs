﻿using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

static class Extensions
{
    public static bool IsText(this HttpContent content)
    {
        var contentType = content.Headers.ContentType;
        if (contentType?.MediaType == null)
        {
            return false;
        }

        return contentType.MediaType.StartsWith("text");
    }
}

namespace VerifyTests.Web
{
    public class RecordingHandler :
        DelegatingHandler
    {
        public ConcurrentQueue<LoggedSend> Sends = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation)
        {
            string? requestText = null;
            var requestContent = request.Content;
            if (requestContent != null)
            {
                if (requestContent.IsText())
                {
                    requestText = await requestContent.ReadAsStringAsync(cancellation);
                }
            }

            var response = await base.SendAsync(request, cancellation);

            var responseContent = response.Content;
            string? responseText = null;
            if (responseContent.IsText())
            {
                responseText = await responseContent.ReadAsStringAsync(cancellation);
            }

            var item = new LoggedSend(requestText, request.Headers,responseText, response.Headers);
            Sends.Enqueue(item);

            return response;
        }
    }
}