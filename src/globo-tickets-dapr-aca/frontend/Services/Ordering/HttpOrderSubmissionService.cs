﻿using GloboTicket.Frontend.Models.Api;
using GloboTicket.Frontend.Models.View;
using GloboTicket.Frontend.Services.ShoppingBasket;

namespace GloboTicket.Frontend.Services.Ordering;

public class HttpOrderSubmissionService : IOrderSubmissionService
{
    private readonly IShoppingBasketService shoppingBasketService;
    private readonly HttpClient orderingClient;

    public HttpOrderSubmissionService(IShoppingBasketService shoppingBasketService, HttpClient orderingClient)
    {
        this.shoppingBasketService = shoppingBasketService;
        this.orderingClient = orderingClient;
    }
    public async Task<Guid> SubmitOrder(CheckoutViewModel checkoutViewModel)
    {

        var lines = await shoppingBasketService.GetLinesForBasket(checkoutViewModel.BasketId);
        var order = new OrderForCreation
        {
            Date = DateTimeOffset.Now,
            OrderId = Guid.NewGuid(),
            Lines = lines.Select(line => new OrderLine() { ConcertId = line.ConcertId, Price = line.Price, TicketCount = line.TicketAmount }).ToList(),
            CustomerDetails = new CustomerDetails()
            {
                Address = checkoutViewModel.Address,
                CreditCardNumber = checkoutViewModel.CreditCard,
                Email = checkoutViewModel.Email,
                Name = checkoutViewModel.Name,
                PostalCode = checkoutViewModel.PostalCode,
                Town = checkoutViewModel.Town,
                CreditCardExpiryDate = checkoutViewModel.CreditCardDate
            }
        };
        // make a synchronous call to the ordering microservice
        var response = await orderingClient.PostAsJsonAsync("order", order);
        // can be a validation error - haven't implemented validation yet
        var s = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        return order.OrderId;
    }
}
