using Microsoft.AspNetCore.Builder;

namespace Kaktos.UserImmediateActions.Extensions
{
    public static class UserImmediateActionsMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware for users immediate actions (E.g: immediate sign-out).
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="app"/> after the operation has completed.</returns>
        public static IApplicationBuilder UseUserImmediateActions(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UserImmediateActionsMiddleware>();
        }
    }
}