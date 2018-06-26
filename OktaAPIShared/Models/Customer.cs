﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OktaAPIShared.Models
{
    [DataContract]
    public class BaseCustomer
    {
        [DataMember(Name = "id")]
        public String Id { get; set; }

        [DataMember(Name = "status")]
        public String Status { get; set; }

        [DataMember(Name = "profile")]
        public Profile Profile { get; set; }

        public BaseCustomer()
        {
            Profile = new Profile();
        }
    }

    [DataContract]
    public class Customer : BaseCustomer
    {
        public Credentials Credentials { get; set; }

        public Customer()
        {
            Credentials = new Credentials();
        }
    }
    
    /// <summary>
    /// Minimum object for Add
    /// </summary>
    [DataContract]
    public class CustomerAdd
    {
        [DataMember(Name = "profile")]
        public Profile Profile { get; set; }

        [DataMember(Name = "credentials")]
        public Credentials Credentials { get; set; }
    }
    
    [DataContract]
    public class Credentials
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8)]
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}