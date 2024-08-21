namespace Lagerhotell.Services;

public class UserServiceLogin
{
    private readonly HttpClient client = new HttpClient();
    private readonly string _baseUrl = "https://localhost:7272/users";
    private AppState appState { get; set; }
    private SessionService sessionService { get; set; }

    public UserServiceLogin(AppState appState, SessionService sessionService)
    {
        this.appState = appState;
        this.sessionService = sessionService;
    }

    public async Task<bool> CheckPhoneNumber(string phoneNumber)
    {
        string url = _baseUrl + "/check-phone/" + phoneNumber;

        HttpResponseMessage response = await client.GetAsync(url);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        CheckPhoneNumber.CheckPhoneNumberResponse deserializedResponse = JsonSerializer.Deserialize<CheckPhoneNumber.CheckPhoneNumberResponse>(await response.Content.ReadAsStringAsync(), options);
        return deserializedResponse.PhoneNumberExistence;
    }

    public async Task<bool> CheckUserExistence(string email)
    {
        string url = _baseUrl + "/check-email/" + email;
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
        return true;
    }
    public async Task<(string, bool)> CheckPassword(string password, string phoneNumber)
    {
        string url = _baseUrl + "/log-in";
        var request = new Login.LoginRequest { PhoneNumber = phoneNumber, Password = password };
        string jsonData = JsonSerializer.Serialize(request);
        StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, stringContent);
        if (response.IsSuccessStatusCode)
        {
            // Read the string content of the response
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Jwt jwt = JsonSerializer.Deserialize<Jwt>(responseString, options);
            return (jwt.Token, true);
        }
        else
        {
            // Handle the error or throw an exception as appropriate
            throw new HttpRequestException($"Invalid response: {response.StatusCode}");
        }
    }

    public async Task<string> GetUserByPhoneNumber(string phoneNumber, string jwtToken)
    {
        string url = _baseUrl + "/get-user-by-phone-number/" + phoneNumber;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        HttpResponseMessage response = await client.GetAsync(url);
        // return User
        string responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        User userDeserialized = JsonSerializer.Deserialize<GetUserByPhoneNumber.GetUserByPhoneNumberResponse>(responseContent, options).User;
        string userId = userDeserialized.Id;
        return userId;
    }

    public async Task<User> GetUserByEmail(string email, string jwtToken)
    {
        string url = _baseUrl + "/get-user-by-email/" + email;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Brukeren eksisterer ikke");
            }
            else
            {
                throw new Exception("En feil oppstod innen innhenting av brukeren");
            }
        }
        string responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        User userDeserialized = JsonSerializer.Deserialize<GetByEmailResponse>(responseContent, options).User;
        return userDeserialized;
    }

    public async Task<(string Token, string UserId)> LoginUser(string phoneNumber, string password)
    {
        bool userExistence = await CheckPhoneNumber(phoneNumber);
        if (!userExistence)
        {
            throw new Exception("Brukeren eksisterer ikke");
        }
        (string token, bool passwordMatch) = await CheckPassword(password, phoneNumber);
        if (!passwordMatch)
        {
            throw new Exception("Feil passord");
        }
        string userId = await GetUserByPhoneNumber(phoneNumber, token);
        await sessionService.AddJwtToLocalStorage(token);
        appState.UserLoggedIn();
        return (token, userId);
    }

    public async Task<string> IsLoginValid(string email, string password)
    {
        string url = _baseUrl + "/log-in-by-email";
        var request = new LoginByEmailRequest(email, password);
        string jsonData = JsonSerializer.Serialize(request);
        StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, stringContent);
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Jwt jwt = JsonSerializer.Deserialize<Jwt>(responseString, options);
            return (jwt.Token);
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Brukeren eksisterer ikke");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("Feil passord eller email");
        }
        else
        {
            throw new Exception($"En feil oppstod under innlogging, status: {response.StatusCode}");
        }
    }

    public async Task<string> LoginUserWithEmail(string email, string password)
    {
        string token;
        User user;
        try
        {
            bool userExistence = await CheckUserExistence(email);
            if (!userExistence)
            {
                throw new KeyNotFoundException("Brukeren eksisterer ikke");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("En feil oppstod under innlogging: " + e);
            throw new Exception("En feil oppstod under innlogging: " + e);
        }
        try
        {
            token = await IsLoginValid(email, password);
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine("Brukeren eksisterer ikke: " + e);
            throw new KeyNotFoundException("Brukeren eksisterer ikke");
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine("Feil passord eller email: " + e);
            throw new InvalidOperationException("Feil passord eller email");
        }
        catch (Exception e)
        {
            Console.WriteLine("En feil oppstod under innlogging: " + e);
            throw new Exception("Noe gikk galt under innlogging av bruker");
        }
        sessionService.AddJwtToLocalStorage(token);
        appState.UserLoggedIn();
        try
        {
            user = await GetUserByEmail(email, token);
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine("Brukeren eksisterer ikke: " + e);
            throw new KeyNotFoundException("Brukeren eksisterer ikke");
        }
        catch (Exception e)
        {
            Console.WriteLine("En feil oppstod under innlogging: " + e);
            throw new Exception("Noe gikk galt under innlogging av bruker");
        }
        return user.Id;
    }

}
