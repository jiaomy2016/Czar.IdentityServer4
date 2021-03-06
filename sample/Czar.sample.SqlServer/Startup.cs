using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Czar.sample.SqlServer.Application.Ids4;
using Czar.sample.SqlServer.Application.Modules;
using Czar.sample.SqlServer.Infrastructure.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Czar.sample.SqlServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.Configure<CzarConfig>(Configuration.GetSection("CzarConfig"));
            services.AddIdentityServer(option => {
                option.PublicOrigin = Configuration["CzarConfig:PublicOrigin"];
            })
                .AddDeveloperSigningCredential()
                .AddDapperStore(option =>
                {
                    option.DbConnectionStrings = Configuration["CzarConfig:DbConnectionStrings"];
                    option.EnableForceExpire = true;
                    option.RedisConnectionStrings = new List<string>() {
                   "192.168.1.111:6379,password=bl123456,defaultDatabase=0,poolsize=50,ssl=false,writeBuffer=10240,connectTimeout=1000,connectRetry=1;"
                     };
                })
                .AddResourceOwnerValidator<CzarResourceOwnerPasswordValidator>()
                .AddProfileService<CzarProfileService>()
                //.AddSecretValidator<JwtSecretValidator>()
                // .AddSecretValidator<PlainTextSharedSecretValidator>()
                ;
            //  .UseMySql();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //使用Autofac进行注入
            var container = new ContainerBuilder();
            container.RegisterModule(new CzarModule());
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
