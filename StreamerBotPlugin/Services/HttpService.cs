// This file is part of the StreamerBotPlugin project.
// 
// Copyright (c) 2022 Dominic Ris
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Loupedeck.StreamerBotPlugin.Services
{
    using System;
    using System.Net.Http;
    using System.Threading;

    using Models.Receive;

    using Newtonsoft.Json;

    public class HttpService : IDisposable
    {
        public static HttpService Instance { get; } = new();
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private HttpService()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this._httpClient = new HttpClient();
            this._httpClient.BaseAddress = new Uri("http://127.0.0.1:7474/");
            this._httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public GetActions GetActions()
        {
            try
            {
                var response = this._httpClient.GetAsync("/GetActions", this._cancellationTokenSource.Token).Result;
                this.OnSuccess.Invoke(this, EventArgs.Empty);
                return JsonConvert.DeserializeObject<GetActions>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                this.OnFailure.Invoke(this, EventArgs.Empty);
                return null;
            }
        }

        public void ExecuteAction(String actionId)
        {
            var json = JsonConvert.SerializeObject(new { action = new { id = actionId } });

            try
            {
                this._httpClient.PostAsync("/DoAction", new StringContent(json), this._cancellationTokenSource.Token).Wait();
                this.OnSuccess.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                this.OnFailure.Invoke(this, EventArgs.Empty);
            }
        }

        public void ExecuteActionValue(String actionId, Object value)
        {
            var json = JsonConvert.SerializeObject(new { action = new { id = actionId }, args = new { value } });

            try
            {
                this._httpClient.PostAsync("/DoAction", new StringContent(json), this._cancellationTokenSource.Token).Wait();
                this.OnSuccess.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                this.OnFailure.Invoke(this, EventArgs.Empty);
            }
        }

        public EventHandler OnSuccess { get; set; } = delegate { };
        public EventHandler OnFailure { get; set; } = delegate { };

        public void Dispose()
        {
            this._cancellationTokenSource.Dispose();
            this._httpClient.Dispose();
        }
    }
}