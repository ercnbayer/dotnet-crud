using System.Reflection;
using dotnetcrud.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace dotnetcrud.Swagger;

public class AuthHeader : IOperationFilter
{


    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // checking end point attributes
        var method = context.MethodInfo;

        var hasAuthAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<ServiceFilterAttribute>()
            .Any(attr => attr.ServiceType == typeof(Authenticate));

        if (!hasAuthAttribute)
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Your Access Token.",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString("TOKEN")
            }
        });

    }
}