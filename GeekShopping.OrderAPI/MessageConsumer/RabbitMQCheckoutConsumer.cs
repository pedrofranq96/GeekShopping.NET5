﻿using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.Repository;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GeekShopping.OrderAPI.MessageConsumer
{
	public class RabbitMQCheckoutConsumer : BackgroundService
	{
		private readonly OrderRepository _repository;
		private IConnection _connection;
		private IModel _channel;

		public RabbitMQCheckoutConsumer(OrderRepository repository)
		{
			_repository = repository;
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};
			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.ThrowIfCancellationRequested();
			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += (chanel, evt) =>
			{
				var content = Encoding.UTF8.GetString(evt.Body.ToArray());
				CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
				ProcessOrder(vo).GetAwaiter().GetResult();
				_channel.BasicAck(evt.DeliveryTag, false);
			};
			_channel.BasicConsume("checkoutqueue", false, consumer);
			return Task.CompletedTask;
		}

		private async Task ProcessOrder(CheckoutHeaderVO vo)
		{
			OrderHeader order = new()
			{
				UserId = vo.UserId,
				FirstName = vo.FirstName,
				LastName = vo.LastName,
				OrderDetails = new List<OrderDetail>(),
				CardNumber = vo.CardNumber,
				CouponCode = vo.CouponCode,
				CVV = vo.CVV,
				DiscountTotal = vo.DiscountTotal,
				Email = vo.Email,
				ExpireMonthYear = vo.ExpireMothYear,
				OrderTime = DateTime.Now,
				PurchaseAmount = vo.PurchaseAmount,
				PaymentStatus = false,
				Phone = vo.Phone,
				Datatime = vo.Datatime
			};

			foreach (var details in vo.CartDetails)
			{
				OrderDetail detail = new()
				{
					ProductId = details.ProductId,
					ProductName = details.Product.Name,
					Price = details.Product.Price,
					Count = details.Count,
				};
				order.CartTotalItens += details.Count;
				order.OrderDetails.Add(detail);
			}

			await _repository.AddOrder(order);
		}
	}
}
