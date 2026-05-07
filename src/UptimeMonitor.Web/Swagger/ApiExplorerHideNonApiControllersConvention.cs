using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace UptimeMonitor.Web.Swagger
{
    public class ApiExplorerHideNonApiControllersConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            action.ApiExplorer.IsVisible = action.Controller.ControllerType.GetCustomAttribute<ApiControllerAttribute>() != null;
        }
    }
}
