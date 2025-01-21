using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client;

public class Transport : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    public bool DebugLoggingEnabled { get; set; } = true;

    public Transport(string baseUrl, HttpClient? client = null)
    {
        _baseUrl = baseUrl;
        _client = client ?? new HttpClient();
        DebugLoggingEnabled = true;
    }

    private void EnsureHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-External-Host", "host.docker.internal" },
            { "X-Ollama-Host", "host.docker.internal" },
            { "X-Ollama-Api-Host", "host.docker.internal" }
        };

        // Remove existing headers to ensure they are set correctly
        foreach (var header in headers.Keys)
        {
            if (_client.DefaultRequestHeaders.Contains(header))
            {
                _client.DefaultRequestHeaders.Remove(header);
            }
        }

        // Add all headers
        foreach (var header in headers)
        {
            _client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        LogDebug($"Headers set: {string.Join(", ", _client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
    }

    private static void LogDebug(Transport transport, string message)
    {
        if (transport.DebugLoggingEnabled)
        {
            Console.WriteLine($"[DEBUG] {message}");
            // Force flush to ensure output is visible
            Console.Out.Flush();
        }
    }

    private void LogDebug(string message)
    {
        LogDebug(this, message);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public string BaseUrl => _baseUrl;
    public HttpClient Client => _client;

    public ApiResponse<T> Get<T>(string path, object? queryParams = null)
    {
        return GetAsync<T>(path, queryParams).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string path, object? queryParams = null, CancellationToken cancellationToken = default)
    {
        try 
        {
            // Ensure proper URL construction
            var baseUrl = _baseUrl.TrimEnd('/');
            var cleanPath = path.TrimStart('/');
            var url = $"{baseUrl}/{cleanPath}";
            
            if (queryParams != null)
            {
                url += $"?{queryParams}";
            }

            LogDebug($"Making GET request to: {url}");
            EnsureHeaders();
            var response = await _client.GetAsync(url, cancellationToken);
            LogDebug($"Response status code: {response.StatusCode}");
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Response content: {content}");
            }
            
            return await CreateApiResponseAsync<T>(this, response);
        }
        catch (Exception ex)
        {
            LogDebug($"Request failed with error: {ex}");
            return new ApiResponse<T>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    public ApiResponse<TResponse> Post<TResponse, TRequest>(string path, TRequest request)
    {
        return PostAsync<TResponse, TRequest>(path, request).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            var cleanPath = path.TrimStart('/');
            var url = $"{baseUrl}/{cleanPath}";
            
            LogDebug($"Making POST request to: {url}");
            EnsureHeaders();
            LogDebug($"Using headers: {string.Join(", ", _client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
            };
            
            // Initial serialization for logging
            var initialJson = System.Text.Json.JsonSerializer.Serialize(request, serializerOptions);
            LogDebug($"Request content (initial): {initialJson}");
            
            // Replace localhost with host.docker.internal in request content
            if (initialJson.Contains("localhost"))
            {
                // Deserialize to dynamic object
                var requestObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(initialJson, serializerOptions);
                if (requestObj != null)
                {
                    var transformed = ReplaceLocalhostInJsonElement(requestObj.RootElement);
                    if (transformed != null)
                    {
                        // Serialize transformed object back to JSON
                        var transformedJson = System.Text.Json.JsonSerializer.Serialize(transformed, serializerOptions);
                        LogDebug($"Request content (transformed): {transformedJson}");
                        
                        // Deserialize back to the original type
                        var transformedRequest = System.Text.Json.JsonSerializer.Deserialize<TRequest>(transformedJson, serializerOptions);
                        if (transformedRequest != null)
                        {
                            request = transformedRequest;
                        }
                    }
                }
            }
            
            // Final serialization and request
            var finalJson = System.Text.Json.JsonSerializer.Serialize(request, serializerOptions);
            Console.WriteLine("\n=== REQUEST DETAILS ===");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Request JSON:\n{finalJson}");
            Console.WriteLine("=== END REQUEST DETAILS ===\n");
            Console.Out.Flush();
            var response = await _client.PostAsJsonAsync(url, request, serializerOptions, cancellationToken);
            LogDebug($"Response status code: {response.StatusCode}");
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Response content: {content}");
            }
            
            return await CreateApiResponseAsync<TResponse>(this, response);
        }
        catch (Exception ex)
        {
            LogDebug($"POST request failed with error: {ex}");
            return new ApiResponse<TResponse>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(string path, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            var cleanPath = path.TrimStart('/');
            var url = $"{baseUrl}/{cleanPath}";
            
            LogDebug($"Making PUT request to: {url}");
            EnsureHeaders();
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
            };
            var requestJson = System.Text.Json.JsonSerializer.Serialize(request, serializerOptions);
            LogDebug($"Request content: {requestJson}");
            var response = await _client.PutAsJsonAsync(url, request, serializerOptions, cancellationToken);
            LogDebug($"Response status code: {response.StatusCode}");
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Response content: {content}");
            }
            
            return await CreateApiResponseAsync<TResponse>(this, response);
        }
        catch (Exception ex)
        {
            LogDebug($"PUT request failed with error: {ex}");
            return new ApiResponse<TResponse>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            var cleanPath = path.TrimStart('/');
            var url = $"{baseUrl}/{cleanPath}";
            
            LogDebug($"Making DELETE request to: {url}");
            EnsureHeaders();
            var response = await _client.DeleteAsync(url, cancellationToken);
            LogDebug($"Response status code: {response.StatusCode}");
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Response content: {content}");
            }
            
            return await CreateApiResponseAsync<T>(this, response);
        }
        catch (Exception ex)
        {
            LogDebug($"DELETE request failed with error: {ex}");
            return new ApiResponse<T>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    public ApiResponse<T> Send<T>(HttpMethod method, string path, object? content = null, Dictionary<string, string>? queryParams = null)
    {
        return SendAsync<T>(method, path, content, queryParams).GetAwaiter().GetResult();
    }

    public async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string path, object? content = null, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            var cleanPath = path.TrimStart('/');
            var url = $"{baseUrl}/{cleanPath}";
            
            if (queryParams != null && queryParams.Count > 0)
            {
                var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{queryString}";
            }

            LogDebug($"Making {method} request to: {url}");
            EnsureHeaders();
            using var httpRequest = new HttpRequestMessage(method, url);
            if (content != null)
            {
                httpRequest.Content = JsonContent.Create(content);
                var contentString = await httpRequest.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Request content: {contentString}");
            }

            var httpResponse = await _client.SendAsync(httpRequest, cancellationToken);
            LogDebug($"Response status code: {httpResponse.StatusCode}");
            
            if (httpResponse.Content.Headers.ContentLength > 0)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                LogDebug($"Response content: {responseContent}");
            }
            
            return await CreateApiResponseAsync<T>(this, httpResponse);
        }
        catch (Exception ex)
        {
            LogDebug($"{method} request failed with error: {ex}");
            return new ApiResponse<T>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    private static async Task<ApiResponse<T>> CreateApiResponseAsync<T>(Transport transport, HttpResponseMessage response)
    {
        var apiResponse = new ApiResponse<T>
        {
            HttpStatusCode = response.StatusCode,
            Result = default
        };

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            LogDebug(transport, $"Response Status: {response.StatusCode}");
            LogDebug(transport, $"Response Content: {content}");
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
                    };
                    
                    if (!string.IsNullOrEmpty(content))
                    {
                        LogDebug(transport, $"Attempting to deserialize content: {content}");
                        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(BatchResponse<>))
                        {
                            var elementType = typeof(T).GetGenericArguments()[0];
                            var arrayType = elementType.MakeArrayType();
                            var array = System.Text.Json.JsonSerializer.Deserialize(content, arrayType, options);
                            if (array != null)
                            {
                                var fromObjectArrayMethod = typeof(T).GetMethod("FromObjectArray");
                                if (fromObjectArrayMethod != null)
                                {
                                    var parameters = fromObjectArrayMethod.GetParameters();
                                    if (parameters.Length == 1)
                                    {
                                        var parameterType = parameters[0].ParameterType;
                                        if (parameterType.IsAssignableFrom(arrayType))
                                        {
                                            var args = new object[] { array };
                                            apiResponse.Result = (T)fromObjectArrayMethod.Invoke(null, args)!;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            apiResponse.Result = System.Text.Json.JsonSerializer.Deserialize<T>(content, options);
                        }
                        LogDebug(transport, $"Deserialized Result: {apiResponse.Result}");
                    }
                    else if (typeof(T) == typeof(object))
                    {
                        // Handle empty response for object type
                        apiResponse.Result = (T)(object)new { };
                    }
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    LogDebug(transport, $"JSON Deserialization Error: {jsonEx}");
                    apiResponse.Error = new ErrorResponse 
                    { 
                        Error = new[] { new Error { Message = $"Failed to deserialize response: {jsonEx.Message}" } }
                    };
                }
            }
            else if (!response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    apiResponse.Error = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(content, options);
                }
                catch (System.Text.Json.JsonException)
                {
                    apiResponse.Error = new ErrorResponse 
                    { 
                        Error = new[] { new Error { Message = content } }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            LogDebug(transport, $"Error processing response: {ex}");
            apiResponse.Error = new ErrorResponse 
            { 
                Error = new[] { new Error { Message = $"Error processing response: {ex.Message}" } }
            };
        }

        return apiResponse;
    }

    private object? ReplaceLocalhostInJsonElement(System.Text.Json.JsonElement element)
    {
        try
        {
            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Object:
                    var result = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        var value = ReplaceLocalhostInJsonElement(property.Value);
                        if (value != null)
                        {
                            result[property.Name] = value;
                        }
                    }
                    return result;

                case System.Text.Json.JsonValueKind.Array:
                    var items = element.EnumerateArray()
                        .Select(item => ReplaceLocalhostInJsonElement(item))
                        .Where(item => item != null)
                        .ToArray();
                    return items.Length > 0 ? items : null;

                case System.Text.Json.JsonValueKind.String:
                    var strValue = element.GetString();
                    if (string.IsNullOrEmpty(strValue)) return null;
                    return strValue.Contains("localhost") 
                        ? strValue.Replace("localhost", "host.docker.internal") 
                        : strValue;

                case System.Text.Json.JsonValueKind.Number:
                    return element.GetRawText();

                case System.Text.Json.JsonValueKind.True:
                    return true;

                case System.Text.Json.JsonValueKind.False:
                    return false;

                case System.Text.Json.JsonValueKind.Null:
                    return null;

                default:
                    return element.GetRawText();
            }
        }
        catch
        {
            return null;
        }
    }
}
