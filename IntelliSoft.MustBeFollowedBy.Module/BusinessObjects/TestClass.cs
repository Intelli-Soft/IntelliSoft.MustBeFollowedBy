//-----------------------------------------------------------------------
// <copyright file="TestClass.cs" company="Intell!Soft">
//     Author: Harald Bacik
//     Copyright (c) Intell!Soft. All rights reserved.
//     Last changed Donnerstag, 1. August 2024 @ 01.08.2024 14:43:46
// </copyright>
//-----------------------------------------------------------------------
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using IntelliSoft.MustBeFollowedBy.Module.Attributes;
using System;
using System.ComponentModel;
using System.Linq;

namespace IntelliSoft.MustBeFollowedBy.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class TestClass : XPBaseObject
    {
        public TestClass(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction() => base.AfterConstruction();

        string mySecondProperty;
        string myFirstProperty;
        bool myIsActive;
        string myName;
        long myId;

        [Browsable(false)]
        [Key(AutoGenerate = true)]
        public long Id
        {
            get => myId;
            set => SetPropertyValue(nameof(Id), ref myId, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => myName;
            set => SetPropertyValue(nameof(Name), ref myName, value);
        }

        public bool IsActive
        {
            get => myIsActive;
            set => SetPropertyValue(nameof(IsActive), ref myIsActive, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [MustBeFollowedBy(nameof(Name))]
        public string FirstProperty
        {
            get => myFirstProperty;
            set => SetPropertyValue(nameof(FirstProperty), ref myFirstProperty, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [MustBeFollowedBy(nameof(Name))]
        public string SecondProperty
        {
            get => mySecondProperty;
            set => SetPropertyValue(nameof(SecondProperty), ref mySecondProperty, value);
        }

    }
}
