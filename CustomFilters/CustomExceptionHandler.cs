using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventsDemo.CustomFilters
{
    public class CustomExceptionHandler: IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(
                new
                {
                    exceptionType = context.Exception.GetType().Name,
                    message = context.Exception.Message,
                    errorCode = System.Net.HttpStatusCode.InternalServerError
                });

            response.ContentLength = result.Length;
            response.WriteAsync(result);

        }
    }
}
