// WP8 client for HelloWorldOAuth2 Web API service
// register a user, login, and make an authenticate call using bearer token issued 
// after login by OAuth token service

using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Windows.Storage;

// web API client utilities 2.2
using System.Net.Http;
using System.Net.Http.Headers;

// models
using HelloWorldOAuth2WP8Client.Models;


namespace HelloWorldOAuth2WP8Client
{
    public partial class MainPage : PhoneApplicationPage
    {
        // base URI for RESTful service (implemented using Web API 2)
        private const String serviceURI = "http://localhost:47385/";        // IIS Express
        // to do use https i.e. setup SSL on IIS express

        // hard-code username and password, not taken from UI yet..
        private const string email = "garyclynch@gc.com";
        private const string password = "Tallaght123_45";                   // strong password required

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        // POST username & password & confirm password to ../api/Account/Register
        private async void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            RegisterModel model = new RegisterModel 
            { 
                Email = email,                              // hard-coded data at moment
                Password = password,
                ConfirmPassword = password,
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serviceURI);                             // base URL for API Controller i.e. RESTFul service

                    client.DefaultRequestHeaders.
                        Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    // POST model to ../api/Account/Register
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/Account/Register", model);   
       
                    // continue
                    if (response.IsSuccessStatusCode)                                                   // 200.299
                    {
                        output.Text = "Registration successful";
                    }
                    else
                    {
                        output.Text = response.StatusCode + " " + response.ReasonPhrase;
                    }
                }
            }
            catch (Exception e1)
            {
                output.Text = e1.ToString();
            }
        }

        // POST to ../token with body specifying username, password and grant_type=password
        private async void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               using ( HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serviceURI);
                    client.DefaultRequestHeaders.
                        Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // setup body for request 
                    string postString = String.Format("username={0}&password={1}&grant_type=password", HttpUtility.HtmlEncode(email), HttpUtility.HtmlEncode(password));
                    ByteArrayContent body = new ByteArrayContent(Encoding.UTF8.GetBytes(postString));

                    // post bytes for request to /Token 
                    HttpResponseMessage response = await client.PostAsync("Token", body);

                    if (response.IsSuccessStatusCode)                                                   // 200.299
                    {
                        // read token response mode result 
                        TokenResponseModel tokenResponse = await response.Content.ReadAsAsync<TokenResponseModel>();

                        // read access token and store in isolated storage for future reference
                        String accessToken = tokenResponse.AccessToken;
                        ApplicationData.Current.LocalSettings.Values["accessToken"] = tokenResponse.AccessToken;

                        // update UI
                        output.Text = "token type: " + tokenResponse.TokenType + " expires at: " + tokenResponse.ExpiresAt;
                    }
                    else
                    {
                        output.Text = response.StatusCode + " " + response.ReasonPhrase;
                    }
                }
            }
            catch (Exception e2)
            {
                output.Text = e2.ToString();
            }
        }

        // GET to /api/Hello (with authorization header set to bearer with access token)
        private async void Hello_Button_Click(object sender, RoutedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(serviceURI);                             // base URL for API Controller i.e. RESTFul service

                client.DefaultRequestHeaders.
                    Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // read token from isolatated storage
                string accessToken = ApplicationData.Current.LocalSettings.Values["accessToken"] as string;

                if (accessToken == null)
                {
                    output.Text = "no token available, get token first";
                    return;
                }

                // set Authorization header 
                // i.e. Authorization: Bearer token
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", accessToken));

                // GET to ../api/hello with header set
                HttpResponseMessage response = await client.GetAsync("api/hello");

                // continue
                if (response.IsSuccessStatusCode)                                                   // 200.299
                {
                    // read result i.e. greeting string
                    String greeting = await response.Content.ReadAsAsync<String>();
                    output.Text = greeting;
                }
                else
                {
                    output.Text = response.StatusCode + " " + response.ReasonPhrase;
                }
            }
        }
    }
}