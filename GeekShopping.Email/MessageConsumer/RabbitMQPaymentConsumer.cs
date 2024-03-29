﻿using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GeekShopping.Email.MessageConsumer
{
	public class RabbitMQPaymentConsumer : BackgroundService
	{
		private readonly EmailRepository _repository;
		private IConnection _connection;
		private IModel _channel;
		private const string ExchangeName = "DirectPaymentUpdate";
		private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
		

		public RabbitMQPaymentConsumer(EmailRepository repository)
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
			_channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
			_channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);
			_channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName, "PaymentEmail");
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.ThrowIfCancellationRequested();
			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += (chanel, evt) =>
			{
				var content = Encoding.UTF8.GetString(evt.Body.ToArray());
				UpdatePaymentResultMessage messages = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content);
				ProcessLogs(messages).GetAwaiter().GetResult();
				_channel.BasicAck(evt.DeliveryTag, false);
			};
			_channel.BasicConsume(PaymentEmailUpdateQueueName, false, consumer);
			return Task.CompletedTask;
		}

		private async Task ProcessLogs(UpdatePaymentResultMessage messages)
		{
			try
			{
				await _repository.LogEmail(messages);
			}
			catch (Exception)
			{

				throw;
			}

			

			
		}
	}
}
