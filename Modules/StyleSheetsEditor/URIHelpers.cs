// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.IO;

namespace UnityEditor.StyleSheets
{
    enum URIValidationResult
    {
        OK,
        InvalidURILocation,
        InvalidURIScheme,
        InvalidURIProjectAssetPath
    }

    static class URIHelpers
    {
        static readonly Uri s_ProjectRootUri = new UriBuilder("project", "").Uri;

        public static URIValidationResult ValidAssetURL(string assetPath, string path, out string errorMessage, out string resolvedProjectRelativePath)
        {
            resolvedProjectRelativePath = null;

            if (string.IsNullOrEmpty(path))
            {
                errorMessage = "Empty URI";
                return URIValidationResult.InvalidURILocation;
            }

            Uri absoluteUri = null;
            // Always treat URIs starting with "/" as implicit project schemes
            if (path.StartsWith("/"))
            {
                var builder = new UriBuilder(s_ProjectRootUri.Scheme, "", 0, path);
                absoluteUri = builder.Uri;
            }
            else if (Uri.TryCreate(path, UriKind.Absolute, out absoluteUri) == false)
            {
                // Resolve a relative URI compared to current file
                Uri assetPathUri = new Uri(s_ProjectRootUri, assetPath);

                if (Uri.TryCreate(assetPathUri, path, out absoluteUri) == false)
                {
                    errorMessage = assetPath;
                    return URIValidationResult.InvalidURILocation;
                }
            }
            else if (absoluteUri.Scheme != s_ProjectRootUri.Scheme)
            {
                errorMessage = absoluteUri.Scheme;
                return URIValidationResult.InvalidURIScheme;
            }

            string projectRelativePath = absoluteUri.LocalPath;

            // Remove any leading "/" as this now used as a path relative to the current directory
            if (projectRelativePath.StartsWith("/"))
            {
                projectRelativePath = projectRelativePath.Substring(1);
            }

            if (string.IsNullOrEmpty(projectRelativePath) || !File.Exists(projectRelativePath))
            {
                errorMessage = projectRelativePath;
                return URIValidationResult.InvalidURIProjectAssetPath;
            }

            resolvedProjectRelativePath = projectRelativePath;
            errorMessage = null;

            return URIValidationResult.OK;
        }
    }
}
