using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace LoggingAutomation.Extensions;

public class ExportApiResponseFilter : IAsyncResultFilter
{
    private readonly string _exportFilePath = "D:\\ExportPath\\HRMS100120\\InputData.json";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var requestBody = string.Empty;
        var responseBody = string.Empty;

        if (!context.HttpContext.Request.HasFormContentType)
        {
            var request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            var originalResponseBodyStream = context.HttpContext.Response.Body;
            using var newResponseBodyStream = new MemoryStream();

            context.HttpContext.Response.Body = newResponseBodyStream;

            try
            {
                await next();

                newResponseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = await new StreamReader(newResponseBodyStream).ReadToEndAsync();
                newResponseBodyStream.Seek(0, SeekOrigin.Begin);

                await newResponseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            finally
            {
                context.HttpContext.Response.Body = originalResponseBodyStream;
            }
        }
        else
        {
            responseBody = "[]";
        }

        var model = new ExportModel()
        {
            Path = context.HttpContext.Request.Path,
            QueryString = context.HttpContext.Request.QueryString.ToString(),
            HttpMethod = context.HttpContext.Request.Method,
            RequestBody = requestBody,
            StatusCode = context.HttpContext.Response.StatusCode,
            ResponseBody = responseBody
        };

        await ExportResponse(model);
    }

    private async Task ExportResponse(ExportModel model)
    {
        var data = new List<ExportModel>();
        if (!File.Exists(_exportFilePath))
        {
            var folderPath = Path.GetDirectoryName(_exportFilePath);
            if (string.IsNullOrWhiteSpace(folderPath)) return;

            Directory.CreateDirectory(folderPath);

            data.Add(model);
            var writeData = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_exportFilePath, writeData);
        }
        else
        {
            var readData = await File.ReadAllTextAsync(_exportFilePath);
            data = JsonSerializer.Deserialize<List<ExportModel>>(readData) ?? [];

            var duplicateData = data.FirstOrDefault(f => f.Equals(model));

            if (duplicateData != null)
            {
                duplicateData.StatusCode = model.StatusCode;
                duplicateData.ResponseBody = model.ResponseBody;
            }
            else
            {
                data.Add(model);
            }
            var writeData = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_exportFilePath, writeData);
        }
    }
}

