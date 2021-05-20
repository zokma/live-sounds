using LiveSounds.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSounds.Api
{
    /// <summary>
    /// Zokma api.
    /// </summary>
    internal class ZokmaApi
    {
        /// <summary>
        /// Regex to check token.
        /// </summary>
        private const string REGEX_TO_CHECK_TOKEN = "^[0-9a-zA-Z]{131,256}$";

        /// <summary>
        /// Sounds api.
        /// </summary>
        private static readonly string SOUNDS_API = $"{AppSettings.ZOKMA_API_URI}/live/v1/sounds";

        /// <summary>
        /// Regex to check token.
        /// This regex is compiled and cached as static field for reuse.
        /// However, it may be called a few times and may be better to instanciate before using.
        /// For now, compiled and cached are adopted.
        /// </summary>
        public static readonly Regex RegexToCheckToken = new Regex(REGEX_TO_CHECK_TOKEN, RegexOptions.Compiled, AppSettings.REGEX_TIMEOUT_NORMAL);

        /// <summary>
        /// Http client.
        /// </summary>
        private static readonly HttpClient httpClient;

        /// <summary>
        /// Token.
        /// </summary>
        private string token;


        static ZokmaApi()
        {
            httpClient = new HttpClient(new SocketsHttpHandler { 
                AllowAutoRedirect = false,
                ConnectTimeout    = AppSettings.HTTP_CONNECTION_TIMEOUT,
            });

            httpClient.Timeout = AppSettings.HTTP_CLIENT_TIMEOUT;

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        /// <summary>
        /// Creates ZokmaApi.
        /// </summary>
        /// <param name="token"></param>
        public ZokmaApi(string token)
        {
            this.token = token;
        }

        /// <summary>
        /// Checks if a token is valid.
        /// </summary>
        /// <param name="token">Token to be checked.</param>
        /// <returns>true if the token is valid.</returns>
        public static bool CheckTokenValid(string token)
        {
            return (!String.IsNullOrEmpty(token) && RegexToCheckToken.IsMatch(token));
        }


        /// <summary>
        /// Validates token.
        /// </summary>
        /// <exception cref="AuthenticationException">Unauthorized.</exception>
        private void ValidateToken()
        {
            if(!CheckTokenValid(this.token))
            {
                throw new AuthenticationException();
            }
        }

        /// <summary>
        /// Creates Sound.
        /// </summary>
        /// <param name="url">Audio rendering url.</param>
        /// <param name="audioItems">Audio items.</param>
        /// <param name="secret">Secret.</param>
        /// <param name="validitySeconds">Validity seconds.</param>
        /// <param name="streamingTitle">Streaming title.</param>
        /// <param name="streamingId">Streaming id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created Sound.</returns>
        /// <exception cref="AuthenticationException">Unauthorized.</exception>
        public async Task<Sound> CreateSound(string url, AudioItem[] audioItems, string secret, int validitySeconds, string streamingTitle, string streamingId, CancellationToken cancellationToken)
        {
            ValidateToken();

            var sound = new Sound { Url = url, Items = audioItems, Secret = secret, ValiditySeconds = validitySeconds, StreamingTitle = streamingTitle, StreamingId = streamingId };
            var utf8  = new UTF8Encoding(false);
            var json  = utf8.GetBytes(JsonSerializer.Serialize(sound, AppSettings.JsonSerializerOptionsForHttpWrite));

            if(json.Length > App.Settings.HttpPostSizeLimitBytes)
            {
                throw new HttpPostSizeTooLargeException();
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, SOUNDS_API);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);

            using var requestContent = new ByteArrayContent(json);

            var contentType = new MediaTypeHeaderValue("application/json");
            contentType.CharSet = "utf-8";

            requestContent.Headers.ContentType = contentType;

            request.Content = requestContent;

            using var response = await httpClient.SendAsync(request, cancellationToken);

            Sound result = null;

            if(response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                using var responseContent = response.Content;

                string body = await responseContent.ReadAsStringAsync();

                result = JsonSerializer.Deserialize<Sound>(body, AppSettings.JsonSerializerOptionsForHttpRead);
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }

            return result;
        }


        /// <summary>
        /// Deletes sound.
        /// </summary>
        /// <param name="id">Resouce id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>true if successfully deleted.</returns>
        /// <exception cref="AuthenticationException">Unauthorized.</exception>
        public async Task<bool> DeleteSound(string id, CancellationToken cancellationToken)
        {
            ValidateToken();

            using var request = new HttpRequestMessage(HttpMethod.Delete, SOUNDS_API + $"/{Uri.EscapeDataString(id)}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.token);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            bool result = false;

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                result = true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }

            return result;
        }

    }
}
