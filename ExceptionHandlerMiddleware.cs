
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace RazorTest
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try {
                await next(context);
            } catch (Exception e) {
                await HandleExceptionAsync(context, e);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception e)
        {          
            var response = context.Response;
            response.ContentType = "application/json; charset=utf-8";
            response.StatusCode = 500;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                Message = e.Message,
            }));
        }
    }
}