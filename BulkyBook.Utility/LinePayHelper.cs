using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class LinePayHelper
{
	private readonly string _channelId = "2007772730";
	private readonly string _channelSecret = "4f3612d2661d53138c6ffa4fc12f6900";
	private readonly string _baseUrl = "https://sandbox-api-pay.line.me";

	public async Task<string> RequestOnlineAPIAsync(
		HttpMethod method,
		string apiPath,
		string queryString = "",
		object data = null,
		CancellationToken cancellationToken = default)
	{
		var nonce = Guid.NewGuid().ToString();
		string message = "";
		string signature = "";

		if (method == HttpMethod.Get)
		{
			message = _channelSecret + apiPath + queryString + nonce;
		}
		else if (method == HttpMethod.Post)
		{
			string jsonData = JsonConvert.SerializeObject(data ?? new { });
			message = _channelSecret + apiPath + jsonData + nonce;
		}

		signature = SignKey(_channelSecret, message);

		var request = new HttpRequestMessage(method, $"{_baseUrl}{apiPath}{(string.IsNullOrEmpty(queryString) ? "" : "?" + queryString)}");

		request.Headers.Add("Accept", "application/json");
		request.Headers.Add("X-LINE-ChannelId", _channelId);
		request.Headers.Add("X-LINE-Authorization", signature);
		request.Headers.Add("X-LINE-Authorization-Nonce", nonce);

		if (method == HttpMethod.Post && data != null)
		{
			string json = JsonConvert.SerializeObject(data);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
		}

		using var client = new HttpClient();
		var response = await client.SendAsync(request, cancellationToken);
		var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
		var jsondoc = JsonDocument.Parse(responseText);
		string webUrl = jsondoc
						.RootElement
						.GetProperty("info")
						.GetProperty("paymentUrl")
						.GetProperty("web")
						.GetString();


		// 可根據需求進行 BigInt 處理（如 Newtonsoft.Json 支援即可）
		return webUrl;
	}

	private string SignKey(string key, string message)
	{
		var keyBytes = Encoding.UTF8.GetBytes(key);
		var messageBytes = Encoding.UTF8.GetBytes(message);

		using var hmac = new HMACSHA256(keyBytes);
		var hashBytes = hmac.ComputeHash(messageBytes);
		return Convert.ToBase64String(hashBytes);
	}
}
