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

    public Transport(string baseUrl, HttpClient? client = null)
    {
        _baseUrl = baseUrl;
        _client = client ?? new HttpClient();
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

            Console.WriteLine($"Making request to: {url}"); // Debug logging
            var response = await _client.GetAsync(url, cancellationToken);
            Console.WriteLine($"Response status code: {response.StatusCode}"); // Debug logging
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response content: {content}"); // Debug logging
            }
            
            return await CreateApiResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed with error: {ex}"); // Debug logging
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
            
            Console.WriteLine($"Making POST request to: {url}"); // Debug logging
            var response = await _client.PostAsJsonAsync(url, request, cancellationToken);
            Console.WriteLine($"Response status code: {response.StatusCode}"); // Debug logging
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response content: {content}"); // Debug logging
            }
            
            return await CreateApiResponseAsync<TResponse>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POST request failed with error: {ex}"); // Debug logging
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
            
            Console.WriteLine($"Making PUT request to: {url}"); // Debug logging
            var response = await _client.PutAsJsonAsync(url, request, cancellationToken);
            Console.WriteLine($"Response status code: {response.StatusCode}"); // Debug logging
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response content: {content}"); // Debug logging
            }
            
            return await CreateApiResponseAsync<TResponse>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PUT request failed with error: {ex}"); // Debug logging
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
            
            Console.WriteLine($"Making DELETE request to: {url}"); // Debug logging
            var response = await _client.DeleteAsync(url, cancellationToken);
            Console.WriteLine($"Response status code: {response.StatusCode}"); // Debug logging
            
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response content: {content}"); // Debug logging
            }
            
            return await CreateApiResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DELETE request failed with error: {ex}"); // Debug logging
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

            Console.WriteLine($"Making {method} request to: {url}"); // Debug logging
            using var httpRequest = new HttpRequestMessage(method, url);
            if (content != null)
            {
                httpRequest.Content = JsonContent.Create(content);
                var contentString = await httpRequest.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Request content: {contentString}"); // Debug logging
            }

            var httpResponse = await _client.SendAsync(httpRequest, cancellationToken);
            Console.WriteLine($"Response status code: {httpResponse.StatusCode}"); // Debug logging
            
            if (httpResponse.Content.Headers.ContentLength > 0)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response content: {responseContent}"); // Debug logging
            }
            
            return await CreateApiResponseAsync<T>(httpResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{method} request failed with error: {ex}"); // Debug logging
            return new ApiResponse<T>
            {
                Error = new ErrorResponse 
                { 
                    Error = new[] { new Error { Message = $"Request failed: {ex.Message}" } }
                }
            };
        }
    }

    private static async Task<ApiResponse<T>> CreateApiResponseAsync<T>(HttpResponseMessage response)
    {
        var apiResponse = new ApiResponse<T>
        {
            HttpStatusCode = response.StatusCode,
            Result = default
        };

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}"); // Debug logging
            Console.WriteLine($"Response Content: {content}"); // Debug logging
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
                    };
                    
                    if (!string.IsNullOrEmpty(content))
                    {
                        Console.WriteLine($"Attempting to deserialize content: {content}"); // Debug logging
                        apiResponse.Result = System.Text.Json.JsonSerializer.Deserialize<T>(content, options);
                        Console.WriteLine($"Deserialized Result: {apiResponse.Result}"); // Debug logging
                    }
                    else if (typeof(T) == typeof(object))
                    {
                        // Handle empty response for object type
                        apiResponse.Result = (T)(object)new { };
                    }
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    Console.WriteLine($"JSON Deserialization Error: {jsonEx}"); // Debug logging
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
            Console.WriteLine($"Error processing response: {ex}"); // Debug logging
            apiResponse.Error = new ErrorResponse 
            { 
                Error = new[] { new Error { Message = $"Error processing response: {ex.Message}" } }
            };
        }

        return apiResponse;
    }
}
