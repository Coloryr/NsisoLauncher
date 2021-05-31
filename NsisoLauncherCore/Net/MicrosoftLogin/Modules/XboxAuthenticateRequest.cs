using Newtonsoft.Json;
using System.Collections.Generic;

namespace NsisoLauncherCore.Net.MicrosoftLogin.Modules
{
    public record XblAuthProperties
    {
        [JsonProperty("AuthMethod")]
        public string AuthMethod { get; set; } = "RPS";

        [JsonProperty("SiteName")]
        public string SiteName { get; set; } = "user.auth.xboxlive.com";

        [JsonProperty("RpsTicket")]
        public string RpsTicket { get; set; }

        public XblAuthProperties(string access_token)
        {
            this.RpsTicket = access_token;
        }
    }

    public record XblAuthRequest : XboxAuthRequest
    {
        [JsonProperty("Properties")]
        public XblAuthProperties Properties { get; set; }
    }

    public record XstsAuthProperties
    {
        [JsonProperty("SandboxId")]
        public string SandBoxId { get; set; } = "RETAIL";

        [JsonProperty("UserTokens")]
        public List<string> UserTokens { get; set; }

        public XstsAuthProperties(string xbl_token)
        {
            UserTokens = new List<string>(1);
            UserTokens.Add(xbl_token);
        }
    }

    public record XstsAuthRequest : XboxAuthRequest
    {
        [JsonProperty("Properties")]
        public XstsAuthProperties Properties { get; set; }
    }

    public record XboxLiveAuthResult
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public Claims DisplayClaims { get; set; }
    }

    public record Claims
    {
        /// <summary>
        /// ?
        /// </summary>
        [JsonProperty("xui")]
        public List<UhsContent> Xui { get; set; }
    }

    public record UhsContent
    {
        [JsonProperty("uhs")]
        public string Uhs { get; set; }
    }

    public abstract record XboxAuthRequest
    {
        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; }

        [JsonProperty("TokenType")]
        public string TokenType { get; set; }
    }
}
