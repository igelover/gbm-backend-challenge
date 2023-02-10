using Gbm.Challenge.API.Controllers;
using Gbm.Challenge.API.Models.Requests;
using Gbm.Challenge.API.Serialization;
using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Domain.Models.DTOs;
using Gbm.Challenge.Domain.Timestamp;
using Gbm.Challenge.Infrastructure.Persistence;
using Gbm.Challenge.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Gbm.Challenge.Tests
{
    public class InvestmentAccountTests : IDisposable
    {
        private const string clientName = "client1";
        private const string apiKey = "apikey-ABC-123-XYZ-001";

        private const decimal cash = 1000m;
        private const int totalShares = 2;
        private const decimal sharePrice = 100m;

        private readonly GbmChallengeApplication<AccountsController> _application;
        private readonly HttpClient _client;
        private readonly DataContext _dbContext;

        public InvestmentAccountTests()
        {
            _application = new GbmChallengeApplication<AccountsController>();
            _client = _application.CreateClient();
            _dbContext = _application.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
            _dbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _client.Dispose();
            _application.Dispose();
        }

        private async Task<string> AquireTokenAsync()
        {
            _client.DefaultRequestHeaders.Add("x-device-shared-secret", apiKey);

            var request = new AuthenticationRequest { ClientName = clientName, ApiKey = apiKey };
            var content = new StringContent(
                JsonConvert.SerializeObject(request, JsonSerializationExtensions.SnakeCaseSettings),
                Encoding.UTF8,
                "application/json"
            );
            var response = await _client.PostAsync("/auth", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> CreateAccountResponse(decimal cash)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme, await AquireTokenAsync()
            );

            var request = new CreateAccountRequest { Cash = cash };
            var content = new StringContent(
                JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"
            );
            return await _client.PostAsync("/accounts", content);
        }

        private async Task<AccountDTO?> CreateAccount(decimal cash)
        {
            var accountResponse = await CreateAccountResponse(cash);
            var accountData = accountResponse.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<AccountDTO>(accountData);
        }
        
        private async Task<(HttpResponseMessage response, CurrentBalanceDTO? balance)> CreateOrder(int accountId, OrderDTO orderDTO)
        {
            var orderContent = PrepareJsonContent(orderDTO);
            var orderResponse = await _client.PostAsync($"/accounts/{accountId}/orders", orderContent);
            var balanceData = orderResponse.Content.ReadAsStringAsync().Result;
            return (orderResponse, JsonConvert.DeserializeObject<CurrentBalanceDTO>(balanceData, JsonSerializationExtensions.SnakeCaseSettings));
        }

        private static StringContent? PrepareJsonContent(OrderDTO order)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    timestamp = Epoch.ToUnix(order.Timestamp),
                    operation = order.Operation,
                    issuer_Name = order.IssuerName,
                    total_shares = order.TotalShares,
                    share_price = order.SharePrice
                }
            );

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task SuccessWhenCreateAccountWithValidData()
        {
            var response = await CreateAccountResponse(cash);
            var result = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(result);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(account);
            Assert.NotEqual(0, account.Id);
            Assert.Equal(cash, account.Cash);
            Assert.Empty(account.Issuers);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public async Task FailWhenCreateAccountWithInvalidData(decimal cash)
        {
            var response = await CreateAccountResponse(cash);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(OperationType.Buy)]
        [InlineData(OperationType.Sell)]
        public async Task SuccessWhenCreatingOrderWithValidData(OperationType operation)
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today.AddHours(12),
                Operation = operation,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice
            };
            var (response, balance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(balance);
            if (operation == OperationType.Buy)
            {
                Assert.Empty(balance.BusinessErrors);
                Assert.Equal(cash - (totalShares * sharePrice), balance.Cash);
            }
        }

        [Fact]
        public async Task SuccessWhenCreatingOrderWithInsufficientBalance()
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today.AddHours(12),
                Operation = OperationType.Buy,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice * 6
            };
            var (response, balance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(balance);
            Assert.Equal(cash, balance.Cash);
            Assert.Single(balance.BusinessErrors);
            Assert.Contains(DomainConstants.InsufficientBalance, balance.BusinessErrors);
        }

        [Fact]
        public async Task SuccessWhenCreatingOrderWithInsufficientStocks()
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today.AddHours(12),
                Operation = OperationType.Sell,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice
            };
            var (response, balance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(balance);
            Assert.Equal(cash, balance.Cash);
            Assert.Single(balance.BusinessErrors);
            Assert.Contains(DomainConstants.InsufficientStocks, balance.BusinessErrors);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        public async Task SuccessWhenCreatingDuplicatedOrder(int minuteOffset)
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today.AddHours(12),
                Operation = OperationType.Buy,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice
            };
            var (_, firstBalance) = await CreateOrder(account!.Id, orderDTO);

            orderDTO.Timestamp = orderDTO.Timestamp.AddMinutes(minuteOffset);
            var (response, secondBalance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(firstBalance);
            Assert.NotNull(secondBalance);
            if (minuteOffset < 5)
            {
                Assert.Single(secondBalance.BusinessErrors);
                Assert.Equal(firstBalance.Cash, secondBalance.Cash);
                Assert.Contains(DomainConstants.DuplicatedOperation, secondBalance.BusinessErrors);
            }
            else
            {
                Assert.Empty(secondBalance.BusinessErrors);
                Assert.NotEqual(firstBalance.Cash, secondBalance.Cash);
                Assert.Equal(cash - (2 * totalShares * sharePrice), secondBalance.Cash);
            }
        }

        [Theory]
        [InlineData(5)]
        [InlineData(12)]
        [InlineData(16)]
        public async Task SuccessWhenCreatingOrderOnClosedMarket(int requestHour)
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today.AddHours(requestHour),
                Operation = OperationType.Buy,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice
            };
            var (response, balance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(balance);
            if (6 < requestHour && requestHour < 15)
            {
                Assert.Empty(balance.BusinessErrors);
                Assert.Equal(cash - (totalShares * sharePrice), balance.Cash);
            }
            else
            {
                Assert.Single(balance.BusinessErrors);
                Assert.Contains(DomainConstants.ClosedMarket, balance.BusinessErrors);
            }
        }

        [Theory]
        [InlineData(OperationType.Buy)]
        [InlineData(OperationType.Sell)]
        public async Task SuccessWhenCreatingOrderWithManyBusinessErrors(OperationType operation)
        {
            var account = await CreateAccount(cash);
            var orderDTO = new OrderDTO
            {
                Timestamp = DateTime.Today,
                Operation = operation,
                IssuerName = "GBM",
                TotalShares = totalShares,
                SharePrice = sharePrice * 6
            };
            var (response, balance) = await CreateOrder(account!.Id, orderDTO);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(balance);
            Assert.Equal(2, balance.BusinessErrors.Count());
            if (operation == OperationType.Buy)
            {
                Assert.Contains(DomainConstants.InsufficientBalance, balance.BusinessErrors);
            }
            if (operation == OperationType.Sell)
            {
                Assert.Contains(DomainConstants.InsufficientStocks, balance.BusinessErrors);
            }
            Assert.Contains(DomainConstants.ClosedMarket, balance.BusinessErrors);
        }
    }
}