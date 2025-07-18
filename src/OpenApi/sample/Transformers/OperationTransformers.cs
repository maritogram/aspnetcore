// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.OpenApi;

namespace Sample.Transformers;

public static class OperationTransformers
{
    public static OpenApiOptions AddHeader(this OpenApiOptions options, string headerName, string defaultValue)
    {
        return options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var schema = OpenApiTypeMapper.MapTypeToOpenApiPrimitiveType(typeof(string));
            schema.Default = defaultValue;
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = headerName,
                In = ParameterLocation.Header,
                Schema = schema
            });
            return Task.CompletedTask;
        });
    }
}
