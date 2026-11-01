using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostsAPI.Test
{
    internal class ApiFactory: WebApplicationFactory<Program>
    {
        private readonly string _environment;

        public ApiFactory(string environment = "Development")
        {
            _environment = environment;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(_environment);
            var conn = Environment.GetEnvironmentVariable("POSTSAPI_TEST_DB");
            builder.UseSetting("ConnectionStrings:DefaultConnection", conn);
            builder.UseSetting("JwtSettings:Key", "asfdasdf5dsa65gf4sadfsda68f49sda16f5ds1fsaodjfoidsnam5465dsf1a65df1");
            builder.UseSetting("JwtSettings:Issuer", "PostsAPI");
            builder.UseSetting("JwtSettings:Audience", "PostsAPI");
            builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        }
    }
}
