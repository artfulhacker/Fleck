using System;
using System.Text;
using System.Text.RegularExpressions;
using Fleck.Interfaces;

namespace Fleck
{
  public class RequestParser : IRequestParser
  {
    const string pattern = @"^(?<method>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\r\n" + // request line
                           @"((?<field_name>[^:\r\n]+):\s(?<field_value>[^\r\n]+)\r\n)+" + //headers
                           @"\r\n" + //newline
                           @"(?<body>.+)?";
                           
    private static readonly Regex _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                           
    public bool IsComplete(byte[] bytes)
    {
      var requestString = Encoding.UTF8.GetString(bytes);
      return _regex.IsMatch(requestString);
    }
    
    public WebSocketHttpRequest Parse(byte[] bytes)
    {
      var body = Encoding.UTF8.GetString(bytes);
      Match match = _regex.Match(body);
      
      var request = new WebSocketHttpRequest{
        Method = match.Groups["method"].Value,
        Path = match.Groups["path"].Value,
        Body = match.Groups["body"].Value,
        Bytes = bytes
      };
      
      var fields = match.Groups["field_name"].Captures;
      var values = match.Groups["field_value"].Captures;
      for (var i = 0; i < fields.Count; i++) {
        var name = fields[i].ToString();
        var value = values[i].ToString();
        request.Headers[name] = value;
      }
      
      return request;
    }
  }
}

