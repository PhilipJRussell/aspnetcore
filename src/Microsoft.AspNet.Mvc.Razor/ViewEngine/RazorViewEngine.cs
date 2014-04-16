﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorViewEngine : IViewEngine
    {
        private static readonly string[] _viewLocationFormats =
        {
            "/Areas/{2}/Views/{1}/{0}.cshtml",
            "/Areas/{2}/Views/Shared/{0}.cshtml",
            "/Views/{1}/{0}.cshtml",
            "/Views/Shared/{0}.cshtml",
        };

        private readonly IVirtualPathViewFactory _virtualPathFactory;

        public RazorViewEngine(IVirtualPathViewFactory virtualPathFactory)
        {
            _virtualPathFactory = virtualPathFactory;
        }

        public IEnumerable<string> ViewLocationFormats
        {
            get { return _viewLocationFormats; }
        }

        public ViewEngineResult FindView([NotNull] IDictionary<string, object> context,
                                         [NotNull] string viewName)
        {
            var viewEngineResult = CreateViewEngineResult(context, viewName);
            return viewEngineResult;
        }

        public ViewEngineResult FindPartialView([NotNull] IDictionary<string, object> context,
                                                [NotNull] string partialViewName)
        {
            return FindView(context, partialViewName);
        }

        private ViewEngineResult CreateViewEngineResult([NotNull] IDictionary<string, object> context,
                                                        [NotNull] string viewName)
        {
            var nameRepresentsPath = IsSpecificPath(viewName);

            if (nameRepresentsPath)
            {
                var view = _virtualPathFactory.CreateInstance(viewName);
                return view != null ? ViewEngineResult.Found(viewName, view) :
                                      ViewEngineResult.NotFound(viewName, new[] { viewName });
            }
            else
            {
                var controllerName = context.GetValueOrDefault<string>("controller");
                var areaName = context.GetValueOrDefault<string>("area");

                var searchedLocations = new List<string>(_viewLocationFormats.Length);
                for (int i = 0; i < _viewLocationFormats.Length; i++)
                {
                    var path = String.Format(CultureInfo.InvariantCulture, _viewLocationFormats[i], viewName, controllerName, areaName);
                    IView view = _virtualPathFactory.CreateInstance(path);
                    if (view != null)
                    {
                        return ViewEngineResult.Found(viewName, view);
                    }
                    searchedLocations.Add(path);
                }

                return ViewEngineResult.NotFound(viewName, searchedLocations);
            }
        }

        private static bool IsSpecificPath(string name)
        {
            char c = name[0];
            return (name[0] == '/');
        }
    }
}
