using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Tests
{//start of namespace

    //this class starts the real API in memory for the tests
    //SQL Server is replaced with an in-memory database so the tests
    //do not need a database server (they also run on GitHub Actions)
    public class RaceDayFactory : WebApplicationFactory<Program>
    {//start of class

        //every factory gets its own empty database
        private readonly string _databaseName = "RaceDayTests-" + Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {//start of method

            builder.UseEnvironment("Testing");

            //set the organiser code that the tests use
            builder.ConfigureAppConfiguration((context, config) =>
            {
                Dictionary<string, string> settings = new Dictionary<string, string>();
                settings["Registration:OrganiserCode"] = TestHelper.OrganiserCode;
                config.AddInMemoryCollection(settings);
            });

            //swap SQL Server for the in-memory database
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor oldSetup = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));

                if (oldSetup != null)
                {//start of if
                    services.Remove(oldSetup);
                }//end of if

                services.AddDbContext<RaceDayContext>(options => options.UseInMemoryDatabase(_databaseName));
            });

        }//end of method

    }//end of class
}//end of namespace
