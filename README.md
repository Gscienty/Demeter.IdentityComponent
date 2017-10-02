# Demeter.IdentityComponent
asp.net core 2.0 Identity Component based on MongoDB, which allows you to build ASP.NET Core 2.0 web applications, including membership, login, role, and user data. With this library, your user's data stored on MongoDB.

## Usage

The library supports [`netstandard2.0`](https://docs.microsoft.com/en-us/dotnet/articles/standard/library).

### Simples

You can find some simples under [./simples](./simples) folder.

### Common

`Startup.cs`:

```csharp
    // some usings
    using Demeter.IdentityComponent.AspNetCore.Extension;

    // some codes
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        
        services.AddDemeterAccount(Configuration, options => {
            //some codes
        });

        //some settings
    }
```

`XXXController.cs`:

```csharp
    //some usings
    using Demeter.IdentityComponent;

    //some codes
    public class XXXController : Controller
    {
        private UserManager<DemeterUserIdentity> _userManager;

        public XXXController(UserManager<DemeterUserIdentity> userManager)
        {
            this._userManager = userManager;
        }

        //some codes
    }
```