using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Tools;

/// <summary>
/// This class is created to replace return of HttpUtility.ParseQueryString("").
/// There is a difference in behaviour of ToString() method in the instance returned by this function,
/// between .NET Framework and .NET Core. The behaviour in .NET Core is wrong - parameter names are
/// not URL-escaped when output to string.
/// </summary>
public class HttpValueCollection : NameValueCollection
{
    public HttpValueCollection()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public override string ToString()
    {
        if (Count == 0) {
            return "";
        }

        var stringBuilder = new StringBuilder();
        foreach (var key in AllKeys) {
            var values = GetValues(key);
            if (values != null) {
                var keyPart = string.IsNullOrEmpty(key) ? null : HttpUtility.UrlEncode(key) + '=';
                foreach (var value in values)
                {
                    stringBuilder.Append(keyPart);
                    stringBuilder.Append(HttpUtility.UrlEncode(value));
                    stringBuilder.Append('&');
                }
            }
        }
        return stringBuilder.ToString(0, stringBuilder.Length - 1);
    }

    public void AddFromQueryString(string query)
    {
        Add(HttpUtility.ParseQueryString(query));
    }

    public static HttpValueCollection ParseQueryString(string query)
    {
        var result = new HttpValueCollection();
        result.AddFromQueryString(query);
        return result;
    }
}