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

        //On frontend, if is authenticated but no user info in store, send GET (if no result send CREATE USER request) and save in store
        //User can update
        /// spells list
        /// name, surname
        /// default view
        /// view settings 

        //Automatically Create SRUser whenever logged in user makes an elevated request 
        //Elevated Request: Create repo, 



        IResult GetUser(string sid)
        {
            var result = new User();
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().Find(c => c.KeycloakId == sid).FirstOrDefault();
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

        IResult DeleteUser(User user)
        {
            bool result;
            using (var store = GetDataStore())
            {
                result = store.GetCollection<User>().DeleteOne(c => c.Id == user.Id);
            }
            return Results.Json(result, _jsonOptions);
        }


        public override void SetRoutes()
        {
            _app.MapGet("/user/{sid}", GetUser);//.RequireAuthorization();
            _app.MapGet("/user/", GetUsers);//.RequireAuthorization();
            _app.MapPut("/user/", CreateUser);//.RequireAuthorization();
            _app.MapPost("/user/", UpdateUser);//.RequireAuthorization();
        }

    }
}

