using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Cstieg.ControllerHelper
{
    /// <summary>
    /// Helper class for controller extensions
    /// </summary>
    public static class ControllerHelper
    {
        /// <summary>
        /// Extension to return JSON success response
        /// </summary>
        public static JsonResult JOk(this Controller controller, object data = null)
        {
            return new JsonResult { Data = new { success = "True", data = data}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Extension to return JSON error response 
        /// </summary>
        /// <param name="message">Error message to pass to front end</param>
        /// <returns>JSON error response</returns>
        public static JsonResult JError(this Controller controller, int errorCode = 400, string message = "", object data = null)
        {
            controller.Response.StatusCode = errorCode;
            return new JsonResult { Data = new { success = "False", message = message, data = data } , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Gets the names of all controllers in project
        /// </summary>
        /// <returns>A list of the names of all the controllers</returns>
        public static List<string> GetControllerNames()
        {
            List<string> controllerNames = new List<string>();
            GetSubClasses<Controller>().ForEach(
                type => controllerNames.Add(type.Name));
            return controllerNames;
        }

        /// <summary>
        /// Helper method to get subclasses of class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<Type> GetSubClasses<T>()
        {
            return Assembly.GetCallingAssembly().GetTypes().Where(
                Type => Type.IsSubclassOf(typeof(T))).ToList();

        }

        /// <summary>
        /// Extension to check whether a controller has a given action
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static bool HasAction(Type T, string actionName)
        {
            try
            {
                var action = T.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                return action != null && action.IsActionResult();
            }
            catch (AmbiguousMatchException)
            {
                return true;
            }
        }

        /// <summary>
        /// Extension to MethodInfo to test whether a method returns a type or subclass of that type
        /// </summary>
        /// <typeparam name="T">The target return Type</typeparam>
        /// <param name="includeSubClasses">Whether to return a positive match for subclasses of the Type also</param>
        /// <usage>bool x = method.IsReturnTypeOf<MyType>();</usage>
        /// <returns>True if the method returns Type T, false if not</returns>
        public static bool IsReturnTypeOf<T>(this MethodInfo method, bool includeSubClasses = true)
        {
            bool isClass = method.ReturnType == typeof(T);
            bool isSubClass = method.ReturnType.IsSubclassOf(typeof(T));
            return includeSubClasses ? (isClass || isSubClass) : isClass;
        }

        /// <summary>
        /// Extension to MethodInfo to test whether a method is an ActionResult
        /// </summary>
        /// <param name="includeAsync">Whether to return a positive match for async Task<ActionResult> also</param>
        /// <returns>True if the method is an ActionResult, false if not</returns>
        public static bool IsActionResult(this MethodInfo method, bool includeAsync = true)
        {
            bool syncType = method.IsReturnTypeOf<ActionResult>();
            bool asyncType = method.IsReturnTypeOf<Task<ActionResult>>();
            return includeAsync ? (syncType || asyncType) : syncType;
        }
    }
}