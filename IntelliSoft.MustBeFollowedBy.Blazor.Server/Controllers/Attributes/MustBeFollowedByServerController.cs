//-----------------------------------------------------------------------
// <copyright file="MustBeFollowedByServerController.cs" company="Intell!Soft">
//     Author: Harald Bacik
//     Copyright (c) Intell!Soft. All rights reserved.
//     Last changed Donnerstag, 1. August 2024 @ 01.08.2024 14:54:53
// </copyright>
//-----------------------------------------------------------------------
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Editors;
using IntelliSoft.MustBeFollowedBy.Module.Attributes;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IntelliSoft.MustBeFollowedBy.Blazor.Server.Controllers.Attributes
{
    public partial class MustBeFollowedByServerController : ViewController<DetailView>
    {
        const string FollowedUpConst = "FollowedUp";
        const string FollowUpIdConst = "FollowUpId";
        const string MustBeFollowedByIdConst = "MustBeFollowedById";

        private PropertyEditor myLastPropertyEditor;

        private Dictionary<string, (string Property, string MustFollowedByProperty, string MustFollowedByPropertyId)> myTuplesOfProperties;

        public MustBeFollowedByServerController() => InitializeComponent();


        private void DetailView_ControlsCreated(object sender, EventArgs e)
        {
            if (View is DetailView detailView)
            {
                var locPropertiesWithMustBeFollowedByAttribute = GetListOfPropertyEditorsWithAttribute();
                if (locPropertiesWithMustBeFollowedByAttribute != null)
                {
                    foreach (var locProperty in locPropertiesWithMustBeFollowedByAttribute)
                    {
                        var locPropertyEditor = detailView.GetItems<PropertyEditor>()
                            .Where(locItem => locItem.PropertyName == locProperty.Name)
                            .FirstOrDefault();

                        if (locPropertyEditor != null)
                        {
                            locPropertyEditor.ControlCreated += PropertyItemCreated;
                            var locAttribute = locProperty.GetCustomAttribute<MustBeFollowedByAttribute>();
                            var locPropertyId = Guid.NewGuid().ToString("N");
                            var locFollowUpPropertyId = Guid.NewGuid().ToString("N");

                            myTuplesOfProperties.Add(
                                locPropertyId,
                                (locProperty.Name, locAttribute.FollowUpPropertyName, locFollowUpPropertyId));

                            var locFollowUpProperty = detailView.GetItems<PropertyEditor>()
                                .Where(locItem => locItem.PropertyName == locAttribute.FollowUpPropertyName)
                                .FirstOrDefault();

                            if (locFollowUpProperty != null)
                            {
                                locFollowUpProperty.ControlCreated += FollowUpPropertyItemCreated;
                            }
                            myTuplesOfProperties.Add(
                                locFollowUpPropertyId,
                                (locAttribute.FollowUpPropertyName, FollowedUpConst, locPropertyId));
                            myLastPropertyEditor = locFollowUpProperty;
                        }
                    }
                }
            }
        }

        private void FollowUpPropertyItemCreated(object sender, EventArgs e)
        {
            if (sender is PropertyEditor locPropertyEditor)
            {
                SetFollowUpPropertyFocus(locPropertyEditor);
                if (myLastPropertyEditor != null && myLastPropertyEditor == locPropertyEditor)
                {
                    CreateDynamicJavaScript();
                }
            }
        }

        private IEnumerable<PropertyInfo> GetListOfPropertyEditorsWithAttribute() => View.CurrentObject
            .GetType()
            .GetProperties()
            .Where(locProperty => locProperty.GetCustomAttributes<MustBeFollowedByAttribute>().Any());

        private void PropertyItemCreated(object sender, EventArgs e)
        {
            if (sender is PropertyEditor locPropertyEditor)
            {
                SetPropertyFocus(locPropertyEditor);
            }
        }

        private void SetFollowUpPropertyFocus(PropertyEditor propertyEditor)
        {
            var locComponentBase = propertyEditor.Control as DevExpress.ExpressApp.Blazor.Components.Models.DxComponentModelBase;

            if (locComponentBase != null)
            {
                var locTuples = myTuplesOfProperties
                    .Where(
                        locItem => locItem.Value.Property == propertyEditor.PropertyName &&
                            locItem.Value.MustFollowedByProperty == FollowedUpConst)
                    .ToList();

                foreach (var locItem in locTuples)
                {
                    var locCssClassName = $" {FollowUpIdConst}:{locItem.Key};";
                    if (locComponentBase.CssClass == null ||
                        !locComponentBase.CssClass.ToString().Contains(locCssClassName))
                    {
                        locComponentBase.CssClass += locCssClassName;
                    }
                }
            }
        }

        private void SetPropertyFocus(PropertyEditor propertyEditor)
        {
            var locComponentBase = propertyEditor.Control as DevExpress.ExpressApp.Blazor.Components.Models.DxComponentModelBase;

            if (locComponentBase != null)
            {
                var locTuple = myTuplesOfProperties
                    .SingleOrDefault(
                        locItem => locItem.Value.Property == propertyEditor.PropertyName &&
                            locItem.Value.MustFollowedByProperty != FollowedUpConst);

                if (locTuple.Key != null)
                {
                    locComponentBase.CssClass += $" {MustBeFollowedByIdConst}:{locTuple.Value.MustFollowedByPropertyId};";
                }
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            myTuplesOfProperties = new();
            View.ControlsCreated += DetailView_ControlsCreated;
        }

        protected override void OnDeactivated()
        {
            foreach (var locTuple in myTuplesOfProperties)
            {
                var locPropertyEditor = View.GetItems<PropertyEditor>()
                    .Where(locItem => locItem.PropertyName == locTuple.Value.Property)
                    .FirstOrDefault();
                if (locPropertyEditor != null)
                {
                    locPropertyEditor.ControlCreated -= PropertyItemCreated;
                }
            }

            myTuplesOfProperties.Clear();
            myTuplesOfProperties = null;
            base.OnDeactivated();
        }

        internal void CreateDynamicJavaScript()
        {
            StringBuilder locJScript = new StringBuilder();

            locJScript.AppendLine("function searchElementsByCssClassPrefix(prefix) {");
            locJScript.AppendLine("  var locAllElements = document.querySelectorAll('*');");
            locJScript.AppendLine("  var locMatchingElements = [];");
            locJScript.AppendLine("  locAllElements.forEach(function(element) {");
            locJScript.AppendLine("    if (typeof element.className === 'string') {");
            locJScript.AppendLine("      var locClasses = element.className.split(' ');");
            locJScript.AppendLine("      locClasses.forEach(function(className) {");
            locJScript.AppendLine("        if (className.startsWith(prefix)) {");
            locJScript.AppendLine("          locMatchingElements.push(element);");
            locJScript.AppendLine("          return; // No need to check other classes for this element");
            locJScript.AppendLine("        }");
            locJScript.AppendLine("      });");
            locJScript.AppendLine("    }");
            locJScript.AppendLine("  });");
            locJScript.AppendLine("  return locMatchingElements;");
            locJScript.AppendLine("}");

            locJScript.AppendLine("function extractValueFromPrefix(element, prefix) {");
            locJScript.AppendLine("  var locClasses = element.className.split(' ');");
            locJScript.AppendLine("  for (var i = 0; i < locClasses.length; i++) {");
            locJScript.AppendLine("    if (locClasses[i].startsWith(prefix)) {");
            locJScript.AppendLine("      return locClasses[i].substring(prefix.length);");
            locJScript.AppendLine("    }");
            locJScript.AppendLine("  }");
            locJScript.AppendLine("  return null;");
            locJScript.AppendLine("}");

            locJScript.AppendLine("function observeAndFocus() {");
            locJScript.AppendLine("  const observer = new MutationObserver(function (mutations) {");
            locJScript.AppendLine("    mutations.forEach(function (mutation) {");
            locJScript.AppendLine("      if (mutation.addedNodes.length > 0) {");
            locJScript.AppendLine("        var locElementsWithPrefix = searchElementsByCssClassPrefix('MustBeFollowedById:');");
            locJScript.AppendLine("        locElementsWithPrefix.forEach(function(element) {");
            locJScript.AppendLine("          var locValue = extractValueFromPrefix(element, 'MustBeFollowedById:');");
            locJScript.AppendLine("          if (locValue != null) {");
            locJScript.AppendLine("            var locElementToFollowUp = searchElementsByCssClassPrefix('FollowUpId:' + locValue);");
            locJScript.AppendLine("            if (locElementToFollowUp.length > 0) {");
            locJScript.AppendLine("              element.addEventListener('focusout', (event) => {");
            locJScript.AppendLine("                locElementToFollowUp[0].focus();");
            locJScript.AppendLine("                locElementToFollowUp[0].select();");
            locJScript.AppendLine("              });");
            locJScript.AppendLine("            }");
            locJScript.AppendLine("          }");
            locJScript.AppendLine("        });");
            locJScript.AppendLine("      }");
            locJScript.AppendLine("    });");
            locJScript.AppendLine("  });");
            locJScript.AppendLine("  const config = { childList: true, subtree: true };");
            locJScript.AppendLine("  observer.observe(document.body, config);");
            locJScript.AppendLine("}");

            locJScript.AppendLine("observeAndFocus();");

            Task.Factory
                .StartNew(
                    async () =>
                    {
                        try
                        {
                            IJSRuntime locJsRuntime = ((BlazorApplication)Application).ServiceProvider
                                .GetRequiredService<IJSRuntime>();
                            await locJsRuntime.InvokeVoidAsync("executeDynamicScript", locJScript.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error, running JavaScript:" + ex.Message);
                        }
                        ;
                    });
        }
    }
}
