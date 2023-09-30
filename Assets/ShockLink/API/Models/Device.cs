﻿using System;

namespace ShockLink.API.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}