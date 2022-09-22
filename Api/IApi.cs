using System;
using System.Text.Json;

namespace SpellsRedApi.Api
{
	public abstract class IApi
	{

        protected WebApplication _app { get; set; }
        protected JsonSerializerOptions _jsonOptions;
        protected string _repoPath;

        public IApi(WebApplication app, JsonSerializerOptions jsonOptions, string repoPath)
        {
            _app = app;
            _jsonOptions = jsonOptions;
            _repoPath = repoPath;
        }

        public abstract void SetRoutes();
    }
}

