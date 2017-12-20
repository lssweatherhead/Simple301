using Umbraco.Web.Routing;
using System;

namespace Simple301.Core
{
    /// <summary>
    /// Content finder to be injected at the start of the Umbraco pipeline that first
    /// looks for any redirects that path the path + query
    /// </summary>
    public class RedirectContentFinder : IContentFinder
    {
        public bool TryFindContent(PublishedContentRequest request)
        {
            //Get the requested URL path + query
            var path = request.Uri.PathAndQuery.ToLower();

            //Check the table
            var matchedRedirect = RedirectRepository.FindRedirect(path);
            if (matchedRedirect == null || string.IsNullOrWhiteSpace(matchedRedirect.NewUrl)) return false;

            //Found one, set the 301 redirect on the request and return
            var redirectUri = GetRoute(matchedRedirect.NewUrl);
            if (redirectUri.IsAbsoluteUri)
            {
                //if is absolute then redirect using the host
                request.SetRedirectPermanent(redirectUri.AbsoluteUri);
            } else
            {
                //redirect is relative, so continue as before
                request.SetRedirectPermanent(matchedRedirect.NewUrl);
            }

            return true;
        }

        private static Uri GetRoute(string url)
        {
            url = url.Trim();
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return new Uri(url, UriKind.Absolute);
            }

            url = url.Substring(url.IndexOf('/'));
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return new Uri(url, UriKind.Relative);
            }

            return null;
        }
    }
}
