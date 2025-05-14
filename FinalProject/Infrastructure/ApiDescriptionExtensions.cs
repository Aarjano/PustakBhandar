using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace FinalProject.Infrastructure
{
    /// <summary>
    /// Extension methods for the ApiDescription class.
    /// </summary>
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Tries to get the method info for an API endpoint.
        /// </summary>
        /// <param name="apiDescription">The API description.</param>
        /// <param name="methodInfo">When this method returns, contains the method info if found; otherwise, null.</param>
        /// <returns>True if the method info was found; otherwise, false.</returns>
        public static bool TryGetMethodInfo(this ApiDescription apiDescription, out MethodInfo methodInfo)
        {
            if (apiDescription.ActionDescriptor.EndpointMetadata != null)
            {
                foreach (var metadata in apiDescription.ActionDescriptor.EndpointMetadata)
                {
                    if (metadata is MethodInfo info)
                    {
                        methodInfo = info;
                        return true;
                    }
                }
            }

            methodInfo = null;
            return false;
        }
    }
} 