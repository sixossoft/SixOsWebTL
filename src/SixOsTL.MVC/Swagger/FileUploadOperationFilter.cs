using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SixOsTL.MVC.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]));

            if (!fileParams.Any()) return;

            IDictionary<string, IOpenApiSchema> properties =
                fileParams.ToDictionary(
                    p => p.Name!,
                    p => (IOpenApiSchema)new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    });

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Properties = properties
                        }
                    }
                }
            };
        }
    }
}