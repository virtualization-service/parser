using System;
using Parser.Processors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;

namespace Parser
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
            services.AddControllers();
            services.AddSingleton<PublishMessage>();
            services.AddSingleton<MessageConsumer>();
            services.AddSingleton<MessageExtractor>();

            services.AddRabbitMQConnection(Configuration);

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ConnectionFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var processors = app.ApplicationServices.GetService<MessageConsumer>();
            var life =  app.ApplicationServices.GetService<IHostApplicationLifetime>();
            life.ApplicationStarted.Register(GetOnStarted(factory, processors));
            life.ApplicationStopping.Register(GetOnStopped(factory, processors));
            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static Action GetOnStarted(ConnectionFactory factory, MessageConsumer processors)
        {
            return () => {processors.Register(factory);};
        }

        private static Action GetOnStopped(ConnectionFactory factory, MessageConsumer processors)
        {
            return () => {processors.DeRegister(factory);};
        }
    }
}
