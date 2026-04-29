using System;
using System.Collections.Specialized;
using System.Web;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public class FetchUrlQueryString : MonoBehaviour
    {
        private const string TokenNameParameter = "token";
        private const string WSUrlParameter = "ws";

        public string Token { get; private set; }
        public string WSUrl { get; private set; }

        private string _launchQueryString;

        private NameValueCollection _queryString;

        private static string GetURLFromQueryStr()
        {
            return Application.absoluteURL;
        }

        private void Start()
        {
            _launchQueryString = ReadURLFromQueryString();
            Token = ParseTokenFromURL();
            WSUrl = ParseWebSocketUrlFromQueryString();
            LoggerHelper.Log("App is running on the url>>>> " + _launchQueryString);
            LoggerHelper.Log($"[{GetType().Name}][{nameof(Start)}] OK, Token: {Token}, WSUrl: {WSUrl}");
        }

        private string ReadURLFromQueryString()
        {
            return GetURLFromQueryStr();
        }

        private string ParseTokenFromURL()
        {
            var url = _launchQueryString;
            if (string.IsNullOrEmpty(url))
            {
                return "URL is empty";
            }

            var uri = new Uri(url);
            string queryString = uri.Query;
            var parametersCollection = HttpUtility.ParseQueryString(queryString);
            _queryString = parametersCollection;
            //LoggerHelper.Log($"[{GetType().Name}][{nameof(ParseTokenFromURL)}] OK, URL: {uri}, Query: {queryString}");
            return parametersCollection.Get(TokenNameParameter);
        }

        private string ParseWebSocketUrlFromQueryString()
        {
            if (_queryString == null)
            {
                var token = ParseTokenFromURL();
            }

            return _queryString != null ? _queryString.Get(WSUrlParameter) : string.Empty;
        }
    }
}
