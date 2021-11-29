using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingListMinimal.Tests;

internal static class HttpHelpers
{
    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        var jsonResponseString = await response.Content.ReadAsStringAsync();
        var entity = JsonConvert.DeserializeObject<T>(jsonResponseString) ?? throw new Exception("Could not deserialize object.");

        return entity;
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient client, string requestUri, object content)
    {
        var json = JsonConvert.SerializeObject(content);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        return await client.PostAsync(requestUri, stringContent);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync(this HttpClient client, string requestUri, object content)
    {
        var json = JsonConvert.SerializeObject(content);
        var stringContent = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        return await client.PutAsync(requestUri, stringContent);
    }
}
