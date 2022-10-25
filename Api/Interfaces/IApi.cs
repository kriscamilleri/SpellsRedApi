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
            _repoPath = properties.Path;
        }
        protected WebApplication _app { get; set; }
        protected JsonSerializerOptions _jsonOptions;
        protected string _repoPath;
        public abstract void SetRoutes();
    }

    public class ApiProperties 
    {
        public ApiProperties(WebApplication app, JsonSerializerOptions jsonOptions, string path)
        {
            App = app;
            Path = path;
            JsonOptions = jsonOptions;
            
        }

        public WebApplication App {get;set;}
        public JsonSerializerOptions JsonOptions {get;set;} 
        public string Path {get;set;}
    }
}

