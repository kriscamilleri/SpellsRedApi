using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using JsonFlatFileDataStore;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
namespace SpellsRedApi.Api
{
    public class UserApi : IApi
    {
        public UserApi(ApiProperties properties) : base(properties)
        {
            
        }

        private DataStore GetDataStore()
        {
            return new DataStore($"Repositories/users.json");
        }

        IResult GetUser(int id)
        {
            var result = new User();
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().Find(c => c.Id == id).First();
            }
            return Results.Json(result, _jsonOptions);
        }

        IResult GetUsers([Microsoft.AspNetCore.Mvc.FromBody] RequestFilter filters)
        {
            User[] result;
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().AsQueryable().ToArray();
            }
            return Results.Json(result, _jsonOptions);
        }

        IResult CreateUser(User user)
        {
            bool result;
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().InsertOne(user);
            }
            return Results.Json(result, _jsonOptions);
        }

        IResult UpdateUser(User user)
        {
            bool result;
            //add check for keycloak permission in claim
            //OR user being updated is same as current claims user
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().UpdateOne(c=> c.Id == user.Id, user);
            }
            return Results.Json(result, _jsonOptions);
        }

        IResult RemoveRepo(User user)
        {
            bool result;
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().UpdateOne(c => c.Id == user.Id, user);
            }
            return Results.Json(result, _jsonOptions);
        }

        public override void SetRoutes()
        {
            _app.MapGet("/user/{id}", GetUser);//.RequireAuthorization();
            _app.MapGet("/user/", GetUsers);//.RequireAuthorization();
            _app.MapPut("/user/", CreateUser);//.RequireAuthorization();
            _app.MapPost("/user/", UpdateUser);//.RequireAuthorization();
        }

    }
}

