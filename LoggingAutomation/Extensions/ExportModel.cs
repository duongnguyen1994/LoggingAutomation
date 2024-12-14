using System.Text.Json.Serialization;

namespace LoggingAutomation.Extensions;

public class ExportModel
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("query_string")]
    public string QueryString { get; set; } = string.Empty;

    [JsonPropertyName("http_method")]
    public string HttpMethod { get; set; } = string.Empty;

    [JsonPropertyName("request_body")]
    public string RequestBody { get; set; } = string.Empty;

    /// <summary>
    /// Used in case of file upload
    /// </summary>
    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("response_body")]
    public string ResponseBody { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return obj is ExportModel model &&
               Path == model.Path &&
               QueryString == model.QueryString &&
               HttpMethod == model.HttpMethod &&
               RequestBody == model.RequestBody;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, QueryString, HttpMethod, RequestBody, StatusCode, ResponseBody);
    }
}
