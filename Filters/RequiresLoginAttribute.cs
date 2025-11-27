using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MineSweeper_MVC.Filters
{
    // Custom action filter to check for login
    public class RequiresLoginAttribute : ActionFilterAttribute
    {
        // OnActionExecuting is a method that checks if the user is logged in before executing the action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the user is logged in by verifying session data
            var session = context.HttpContext.Session;

            // If not logged in, redirect to the login page
            if (session.GetString("Username") == null)
            {
                // Provide user-friendly feedback
                var controller = (Controller)context.Controller;
                controller.TempData["LoginRequired"] = "Please log in to continue.";

                // Redirect to the login page
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }
    }
}
