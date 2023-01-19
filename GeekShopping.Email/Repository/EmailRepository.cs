﻿using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GeekShopping.Email.Repository
{
	public class EmailRepository : IEmailRepository
	{
		private readonly DbContextOptions<SqlServerContext> _context;

		public EmailRepository(DbContextOptions<SqlServerContext> context)
		{
			_context = context;
		}

		public async Task LogEmail(UpdatePaymentResultMessage message)
		{
			EmailLog email = new EmailLog()
			{
				Email = message.Email,
				SentDate = DateTime.Now,
				Log = $"Order - {message.OrderId} has been created successfully!"
			};
			await using var _db = new SqlServerContext(_context);
			_db.Emails.Add(email);
			await _db.SaveChangesAsync();
		}

	}
}
