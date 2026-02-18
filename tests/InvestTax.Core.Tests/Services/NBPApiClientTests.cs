using FluentAssertions;
using InvestTax.Core.Enums;
using InvestTax.Core.Exceptions;
using InvestTax.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace InvestTax.Core.Tests.Services;

public class NBPApiClientTests
{
    private readonly Mock<ILogger<NBPApiClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly NBPApiClient _client;

    public NBPApiClientTests()
    {
        _loggerMock = new Mock<ILogger<NBPApiClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.nbp.pl")
        };
        _client = new NBPApiClient(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetExchangeRateAsync_PLNCurrency_ReturnsOne()
    {
        var date = new DateOnly(2024, 1, 15);

        var rate = await _client.GetExchangeRateAsync(Currency.PLN, date);

        rate.Should().Be(1.0m);
        _httpMessageHandlerMock.Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetExchangeRateAsync_ValidCurrencyAndDate_ReturnsRate()
    {
        var date = new DateOnly(2024, 1, 15);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        var rate = await _client.GetExchangeRateAsync(Currency.USD, date);

        rate.Should().Be(expectedRate);
        VerifyHttpRequest("https://api.nbp.pl/api/exchangerates/rates/a/usd/2024-01-15/?format=json");
    }

    [Fact]
    public async Task GetExchangeRateAsync_WeekendDate_FallsBackToPreviousDate()
    {
        var weekendDate = new DateOnly(2024, 1, 13); // Saturday
        var previousFridayDate = new DateOnly(2024, 1, 12);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        var rate = await _client.GetExchangeRateAsync(Currency.USD, weekendDate);

        rate.Should().Be(expectedRate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_MultipleWeekendDays_SearchesBackward()
    {
        var monday = new DateOnly(2024, 1, 15);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        var sequence = SetupHttpResponseSequence();
        
        sequence.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound }); // Monday
        sequence.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound }); // Sunday
        sequence.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound }); // Saturday
        sequence.ReturnsAsync(new HttpResponseMessage 
        { 
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent)
        }); // Friday

        var rate = await _client.GetExchangeRateAsync(Currency.USD, monday);

        rate.Should().Be(expectedRate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_NoRateWithin7Days_ThrowsException()
    {
        var date = new DateOnly(2024, 1, 1);

        var sequence = SetupHttpResponseSequence();
        for (int i = 0; i <= 7; i++)
        {
            sequence.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });
        }

        var act = async () => await _client.GetExchangeRateAsync(Currency.USD, date);

        await act.Should().ThrowAsync<NBPApiException>()
            .WithMessage("*within 7 days*");
    }

    [Fact]
    public async Task GetExchangeRateAsync_HttpRequestException_ThrowsNBPApiException()
    {
        var date = new DateOnly(2024, 1, 15);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var act = async () => await _client.GetExchangeRateAsync(Currency.USD, date);

        var exception = await act.Should().ThrowAsync<NBPApiException>();
        exception.WithMessage("*Failed to fetch exchange rate*");
        exception.Which.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetExchangeRateAsync_EmptyRateData_ThrowsException()
    {
        var date = new DateOnly(2024, 1, 15);
        var responseContent = JsonSerializer.Serialize(new
        {
            table = "A",
            currency = "dolar amerykański",
            code = "USD",
            rates = Array.Empty<object>()
        });

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        var act = async () => await _client.GetExchangeRateAsync(Currency.USD, date);

        await act.Should().ThrowAsync<NBPApiException>()
            .WithMessage("*No exchange rate data returned*");
    }

    [Fact]
    public async Task GetExchangeRateAsync_NullRates_ThrowsException()
    {
        var date = new DateOnly(2024, 1, 15);
        var responseContent = @"{""table"":""A"",""currency"":""dolar amerykański"",""code"":""USD""}";

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        var act = async () => await _client.GetExchangeRateAsync(Currency.USD, date);

        await act.Should().ThrowAsync<NBPApiException>();
    }

    [Fact]
    public async Task GetExchangeRatesAsync_MultipleDates_ReturnsAllRates()
    {
        var dates = new List<DateOnly>
        {
            new DateOnly(2024, 1, 15),
            new DateOnly(2024, 2, 15),
            new DateOnly(2024, 3, 15)
        };

        var rate1 = 4.0234m;
        var rate2 = 4.1234m;
        var rate3 = 4.2234m;

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, CreateNBPResponse(rate1)))
            .ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, CreateNBPResponse(rate2)))
            .ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, CreateNBPResponse(rate3)));

        var rates = await _client.GetExchangeRatesAsync(Currency.USD, dates);

        rates.Should().HaveCount(3);
        rates[dates[0]].Should().Be(rate1);
        rates[dates[1]].Should().Be(rate2);
        rates[dates[2]].Should().Be(rate3);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_EmptyDateList_ReturnsEmptyDictionary()
    {
        var dates = new List<DateOnly>();

        var rates = await _client.GetExchangeRatesAsync(Currency.USD, dates);

        rates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExchangeRateAsync_DifferentCurrencies_FormatsCorrectly()
    {
        var date = new DateOnly(2024, 1, 15);
        var expectedRate = 4.5m;
        var responseContent = CreateNBPResponse(expectedRate);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        await _client.GetExchangeRateAsync(Currency.EUR, date);

        VerifyHttpRequest("https://api.nbp.pl/api/exchangerates/rates/a/eur/2024-01-15/?format=json");
    }

    [Fact]
    public async Task GetExchangeRateAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var date = new DateOnly(2024, 1, 15);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _client.GetExchangeRateAsync(Currency.USD, date, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("2024-12-31")]
    [InlineData("2024-06-15")]
    public async Task GetExchangeRateAsync_VariousDates_FormatsDateCorrectly(string dateString)
    {
        var date = DateOnly.Parse(dateString);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        await _client.GetExchangeRateAsync(Currency.USD, date);

        VerifyHttpRequest($"https://api.nbp.pl/api/exchangerates/rates/a/usd/{dateString}/?format=json");
    }

    [Fact]
    public async Task GetExchangeRateAsync_ServerError_RetriesWithBackoff()
    {
        var date = new DateOnly(2024, 1, 15);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError })
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable })
            .ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, responseContent));

        var rate = await _client.GetExchangeRateAsync(Currency.USD, date);

        rate.Should().Be(expectedRate);
        
        _httpMessageHandlerMock.Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetExchangeRateAsync_TooManyRequests_RetriesWithBackoff()
    {
        var date = new DateOnly(2024, 1, 15);
        var expectedRate = 4.0234m;
        var responseContent = CreateNBPResponse(expectedRate);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.TooManyRequests })
            .ReturnsAsync(CreateHttpResponse(HttpStatusCode.OK, responseContent));

        var rate = await _client.GetExchangeRateAsync(Currency.USD, date);

        rate.Should().Be(expectedRate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_HighPrecisionRate_PreservesDecimals()
    {
        var date = new DateOnly(2024, 1, 15);
        var expectedRate = 4.023456789m;
        var responseContent = CreateNBPResponse(expectedRate);

        SetupHttpResponse(HttpStatusCode.OK, responseContent);

        var rate = await _client.GetExchangeRateAsync(Currency.USD, date);

        rate.Should().Be(expectedRate);
    }

    private static string CreateNBPResponse(decimal rate)
    {
        return JsonSerializer.Serialize(new
        {
            table = "A",
            currency = "dolar amerykański",
            code = "USD",
            rates = new[]
            {
                new
                {
                    no = "010/A/NBP/2024",
                    effectiveDate = "2024-01-15",
                    mid = rate
                }
            }
        });
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateHttpResponse(statusCode, content));
    }

    private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        };
    }

    private Moq.Language.ISetupSequentialResult<Task<HttpResponseMessage>> SetupHttpResponseSequence()
    {
        return _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    private void VerifyHttpRequest(string expectedUrl)
    {
        _httpMessageHandlerMock.Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());
    }
}
