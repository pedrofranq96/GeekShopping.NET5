﻿using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
	public class DbInitializer : IDbInitializer
	{
		private readonly SqlServerContext _context;
		private readonly UserManager<ApplicationUser> _user;
		private readonly RoleManager<IdentityRole> _role;

		public DbInitializer(SqlServerContext context, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role)
		{
			_context = context;
			_user = user;
			_role = role;
		}

		public void Initialize()
		{
			if (_role.FindByNameAsync(IdentityConfiguration.Admin).Result != null) return;
			_role.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
			_role.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

			ApplicationUser admin = new ApplicationUser()
			{
				UserName = "pedro-admin",
				Email = "pedro-admin@gmail.com",
				EmailConfirmed = true,
				PhoneNumber = "+55 (21)11234-5687",
				FirtsName = "Pedro",
				LastName = "Admin"
			};

			_user.CreateAsync(admin, "Pedro@123").GetAwaiter().GetResult();
			_user.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();

			var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
			{
				new Claim(JwtClaimTypes.Name, $"{admin.FirtsName} {admin.LastName}"),
				new Claim(JwtClaimTypes.GivenName, admin.FirtsName),
				new Claim(JwtClaimTypes.FamilyName, admin.LastName),
				new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)
			}).Result;


			ApplicationUser client = new ApplicationUser()
			{
				UserName = "pedro-client",
				Email = "pedro-client@gmail.com",
				EmailConfirmed = true,
				PhoneNumber = "+55 (21)11234-5687",
				FirtsName = "Pedro",
				LastName = "Client"
			};

			_user.CreateAsync(client, "Pedro@123").GetAwaiter().GetResult();
			_user.AddToRoleAsync(client, IdentityConfiguration.Client).GetAwaiter().GetResult();

			var clientClaims = _user.AddClaimsAsync(client, new Claim[]
			{
				new Claim(JwtClaimTypes.Name, $"{client.FirtsName} {client.LastName}"),
				new Claim(JwtClaimTypes.GivenName, client.FirtsName),
				new Claim(JwtClaimTypes.FamilyName, client.LastName),
				new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)
			}).Result;
		}
	}
}
