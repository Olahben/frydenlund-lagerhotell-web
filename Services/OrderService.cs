global using System.Net.Http.Headers;
using LagerhotellAPI.Models.DomainModels.Validators;
using LagerhotellAPI.Models.ValueTypes;

namespace Lagerhotell.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;
    private readonly SessionService _sessionService;
    private readonly string url = "https://localhost:7272/orders";
    private OrderValidator _orderValidator;

    public OrderService(HttpClient httpClient, SessionService sessionService)
    {
        _httpClient = httpClient;
        _sessionService = sessionService;
        _orderValidator = new OrderValidator();
    }

    public async Task<bool> ValidateOrder(Order order)
    {
        var validationResult = await _orderValidator.ValidateAsync(order);
        return validationResult.IsValid;
    }

    public async Task<string> CreateOrder(Order order)
    {
        string endpointUrl = url + "/add";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        var newPendingOrder = order with { Status = OrderStatus.Pending };
        AddOrderRequest request = new AddOrderRequest { Order = newPendingOrder };
        string jsonData = JsonSerializer.Serialize(request);
        StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(endpointUrl, stringContent);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<AddOrderResponse>(responseContent, options).OrderId;
        }
        else
        {
            throw new InvalidOperationException($"Something went wrong when adding order, code: {response.StatusCode}");
        }
    }

    public async Task<Order> GetOrderByIdAsync(string id)
    {
        string endpointUrl = url + $"/{id}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Order>(responseContent, options);
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("The order was not found");
        }
        else
        {
            throw new InvalidOperationException($"Something went wrong when fetching order, code: {response.StatusCode}");
        }
    }
    public async Task<List<Order>> GetUserOrdersAsync(string userId)
    {
        string endpointUrl = url + $"?userId={userId}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Order> orders = JsonSerializer.Deserialize<List<Order>>(responseContent, options);
            // Remove orders that has their status as deleted
            // Sort based on status, order period, start date
            return orders
                .Where(o => o.Status != OrderStatus.Deleted)
                .OrderByDescending(o => o.Status != OrderStatus.Cancelled)
                .ThenByDescending(o => o.OrderPeriod.OrderDate)
                .ToList();
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("The user is not authorized to view these orders");
        }
        else
        {
            throw new InvalidOperationException($"Something went wrong when fetching orders, code: {response.StatusCode}");
        }
    }

    public async Task<List<Order>> GetAllOrders(int? skip, int? take)
    {
        string endpointUrl = url + $"?skip={skip}&take={take}";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Order> orders = JsonSerializer.Deserialize<List<Order>>(responseContent, options);
            return orders;
        }
        else
        {
            throw new InvalidOperationException($"Something went wrong when fetching orders, code: {response.StatusCode}");
        }
    }

    public async Task<OrderPeriod> CancelOrder(string id)
    {
        var request = new CancelOrderRequest(id);
        string url = this.url + $"/cancel";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PutAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            string deserializedResponseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserializedResponse = JsonSerializer.Deserialize<CancelOrderResponse>(deserializedResponseString, options);
            return deserializedResponse.OrderPeriod;
        }
        else if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("The order is already cancelled");
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("The order was not found");
        }
        else
        {
            throw new Exception("Something went wrong");
        }
    }

    public async Task ConfirmOrder(Order order)
    {
        Order newOrder = order with { Status = OrderStatus.Confirmed };
        var request = new ConfirmOrderRequest(newOrder);
        string url = this.url + $"/update";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _sessionService.GetJwtFromLocalStorage());
        string jsonData = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("The order was not found");
            }
            else
            {
                throw new Exception($"Something went wrong when updating order, code: {response.StatusCode}");
            }
        }
    }
}
