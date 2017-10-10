# MyCodeCamp in ASP.NET Core 2.0

App follows Shawn Wildermuths plural sight course  for developing apps with ASP.NET Core 1.1.

>Note App requires following packages to run for **Core 2.0**

```xml
	<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore" Version="2.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="2.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="2.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.0" />
```

## How to run

- First get all required NuGet packages

- ```dotnet restore```

- Comment out `CampDbInitializer` and `CampIdentityInitializer` classes.

- Comment out `services.AddTransient<CampDbInitializer>();` and `services.AddTransient<CampIdentityInitializer>();` from `ConfigureServices` method in *Startup.cs*

- Comment out `dbSeeder.Seed().Wait();` and `identitySeeder.Seed().Wait();` from `Configure` method in *Startup.cs*

- ```update-database```

- uncomment all of the above

- run the application

>Note - needs to add `DesignTimeDbContextFactory` class in the context in order to run in ASP.NET Core 2.0.

```csharp
 public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CampContext>
{
    public CampContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Developement.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<CampContext>();
        var connectionString = configuration.GetConnectionString("Data:ConnectionString");
        builder.UseSqlServer(connectionString);

        return new CampContext(builder.Options, configuration);
    }
}
```