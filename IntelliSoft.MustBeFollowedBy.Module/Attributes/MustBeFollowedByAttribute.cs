//-----------------------------------------------------------------------
// <copyright file="MustBeFollowedByAttribute.cs" company="Intell!Soft">
//     Author: Harald Bacik
//     Copyright (c) Intell!Soft. All rights reserved.
//     Last changed Donnerstag, 1. August 2024 @ 01.08.2024 14:40:55
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;

namespace IntelliSoft.MustBeFollowedBy.Module.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MustBeFollowedByAttribute : Attribute
    {
        public MustBeFollowedByAttribute(string followUpPropertyName) => FollowUpPropertyName = followUpPropertyName;
        public string FollowUpPropertyName { get; set; }
    }
}
