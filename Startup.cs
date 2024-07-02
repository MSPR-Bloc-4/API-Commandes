using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Grpc.Auth;
using Order_Api.Configuration;
using Order_Api.Repository;
using Order_Api.Repository.Interface;
using PubSubLibrary;
using Grpc.Core;
using Order_Api.Service;

namespace Order_Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FirebaseConfig>(_configuration.GetSection("FirebaseConfig"));
            var firebaseConfig = _configuration.GetSection("FirebaseConfig").Get<FirebaseConfig>();

            GoogleCredential credential;
            using (var stream = new FileStream(firebaseConfig.ServiceAccountPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream);
            }

            FirestoreDbBuilder builder = new FirestoreDbBuilder
            {
                ProjectId = firebaseConfig.ProjectId,
                DatabaseId = "order",
                Credential = credential
            };

            FirestoreDb db = builder.Build();

            // Add FirestoreDb as a singleton service
            services.AddSingleton(db);

            // Add PubSubService as a singleton service
            services.AddSingleton<PubSubService>(provider =>
            {
                return new PubSubService(firebaseConfig.ProjectId, credential.ToChannelCredentials());
            });
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(firebaseConfig.ProjectId, "user-deleted-sub");
            services.AddSubscriberClient(builder =>
            {
                builder.SubscriptionName = subscriptionName;
                builder.CredentialsPath = firebaseConfig.ServiceAccountPath;
                // Other settings to customize the client.
            });
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddHostedService<SubscriberService>();

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddAuthorization();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }
        

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });
        }
    }
}
