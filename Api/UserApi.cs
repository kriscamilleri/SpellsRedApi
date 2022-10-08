﻿using System;
using System.Text.Json;
using JsonFlatFileDataStore;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
namespace SpellsRedApi.Api
{
    public class UserApi : IApi
    {
        public UserApi(ApiProperties properties)
            : base(properties) { }

        IResult GetUser(int id)
        {
            User result = new User();
            using (var store = new DataStore($"Repositories/users.json"))
            {
                result = store.GetCollection<User>().AsQueryable().First(c => c.Id == id);
            }
            return Results.Json(result, _jsonOptions);
        }

        public override void SetRoutes()
        {
            _app.MapGet("/user/{id}", GetUser).RequireAuthorization();
        }

    }
}

