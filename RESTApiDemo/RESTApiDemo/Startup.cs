using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RESTApiDemo.Models;

namespace RESTApiDemo
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<Models.SumTotalSettings>(Configuration.GetSection("SumTotalSettings"));
      var sumtSettings = Configuration.GetSection("SumTotalSettings").Get<SumTotalSettings>();
      services.AddControllersWithViews();
      services.AddAuthentication(opt =>
      {
        opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = "oauth";
      })
      .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddOAuth("oauth", opt =>
      {
        opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.CallbackPath = new PathString("/signin-oauth");
        opt.AuthorizationEndpoint = $"{ sumtSettings.BaseUrl }/apisecurity/connect/authorize";
        opt.TokenEndpoint = $"{ sumtSettings.BaseUrl }/apisecurity/connect/token";
        opt.Scope.Add("allapis");
        opt.ClientId = sumtSettings.ClientId;
        opt.ClientSecret = sumtSettings.ClientSecret;
        opt.SaveTokens = true;
      });
      services.AddMemoryCache();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseCookiePolicy();
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
