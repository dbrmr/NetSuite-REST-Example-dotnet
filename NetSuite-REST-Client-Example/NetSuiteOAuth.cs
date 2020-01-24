using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetSuiteRESTExample
{
    public class NetSuiteConfig
    {
        public string AccountId { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenId { get; set; }
        public string TokenSecret { get; set; }
    }

    public interface INetSuiteOAuth
    {
        string AccountId { get; }

        string GetAuthorizationHeader(string httpMethod,
            Uri uri, Dictionary<string, string> parameters = null);
    }

    public class NetSuiteOAuth : INetSuiteOAuth
    {
        private readonly IOptions<NetSuiteConfig> _netSuiteConfig;
        static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        public string AccountId { get; }

        public NetSuiteOAuth(IOptions<NetSuiteConfig> config)
        {
            _netSuiteConfig = config ?? throw new ArgumentNullException(nameof(config));

            AccountId = _netSuiteConfig.Value.AccountId;
        }

        public string GetAuthorizationHeader(string httpMethod,
            Uri uri, Dictionary<string, string> parameters = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            if (parameters == null) parameters = new Dictionary<string, string>();

            var prms = new Dictionary<string, string>(parameters);
            prms["oauth_token"] = _netSuiteConfig.Value.TokenId;
            prms["oauth_nonce"] = GetNonce();
            prms["oauth_timestamp"] = GetUnixTimestamp();
            prms["oauth_version"] = "1.0";
            prms["oauth_consumer_key"] = _netSuiteConfig.Value.ConsumerKey;
            prms["oauth_signature_method"] = "HMAC-SHA256";
            prms["oauth_signature"] = GetSignature(httpMethod, uri, prms);

            return "OAuth realm=\""
                + Uri.EscapeDataString(_netSuiteConfig.Value.AccountId.ToUpperInvariant()) + "\", "
                + string.Join(", ", prms
                    .Where(a => a.Key.StartsWith("oauth_"))
                    .Select(kvp => Uri.EscapeDataString(kvp.Key) + "=\"" + Uri.EscapeDataString(kvp.Value) + "\"")
            );
        }

        private string GetSignature(string httpMethod, Uri uri, Dictionary<string, string> parameters)
        {
            string prms = string.Join("&",
                parameters.OrderBy(a => a.Key)
                          .Select(kvp => string.Format("{0}={1}",
                                Uri.EscapeDataString(kvp.Key),
                                Uri.EscapeDataString(kvp.Value))));

            string baseString = string.Format("{0}&{1}&{2}",
                httpMethod,
                Uri.EscapeDataString(uri.AbsoluteUri.Split('?').First()), // without Query Parameters
                Uri.EscapeDataString(prms));

            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(string.Format("{0}&{1}",
                _netSuiteConfig.Value.ConsumerSecret,
                _netSuiteConfig.Value.TokenSecret)));

            return Convert.ToBase64String(hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(baseString)));
        }

        private static string GetUnixTimestamp() =>
            ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

        private string GetNonce()
        {
            byte[] data = new byte[20];
            _rng.GetBytes(data);
            return Math.Abs(BitConverter.ToInt32(data, 0)).ToString();
        }
    }
}
