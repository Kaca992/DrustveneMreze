using System;
using System.Collections.Generic;
using DrustveneMrezev3.Managers;
using Facebook;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrustveneMrezev3.MongoDB_objects
{
    public class UserInformation
    {       
        [BsonId]
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, DateOnly = true)]
        public DateTime Birthday { get; set; }
        public List<MovieLike> MovieLikes { get; set; }        
    }
}