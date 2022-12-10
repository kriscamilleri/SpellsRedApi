using System;
using System.Security.Claims;
using System.Text.Json;

namespace SpellsRedApi.Api
{
    public abstract class IApi
    {
        public IApi(ApiProperties properties)
        {
            _app = properties.App;
            _jsonOptions = properties.JsonOptions;
            _paths = properties.Paths;
        }
        protected WebApplication _app { get; set; }
        protected JsonSerializerOptions _jsonOptions;
        protected Paths _paths;
        public abstract void SetRoutes();
    }

    public class ApiProperties
    {
        public ApiProperties(WebApplication app, JsonSerializerOptions jsonOptions, IConfiguration configuration)
        {
            App = app;
            Paths = new Paths(configuration);
            JsonOptions = jsonOptions;
        }

        public WebApplication App { get; set; }
        public JsonSerializerOptions JsonOptions { get; set; }
        public Paths Paths { get; set; }
    }

    public class Paths
    {
        public Paths(IConfiguration configuration)
        {
            Repository = configuration["Paths:Repository"];
            RedSpell = configuration["Paths:RedSpell"];
            LegacySpell = configuration["Paths:LegacySpell"];
            GiddySpell = configuration["Paths:GiddySpell"];
        }

        public string Repository { get; set; }
        public string RedSpell { get; set; }
        public string LegacySpell { get; set; }
        public string GiddySpell { get; set; }
    }
}

