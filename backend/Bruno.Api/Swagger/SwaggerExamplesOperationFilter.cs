using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bruno.Api.Swagger;

public sealed class SwaggerExamplesOperationFilter : IOperationFilter
{
    private static readonly Guid SampleVehicleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SampleCustomerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SampleBookingId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var route = context.ApiDescription.RelativePath ?? string.Empty;
        var method = context.ApiDescription.HttpMethod ?? string.Empty;

        if (route.Contains("vehicles", StringComparison.OrdinalIgnoreCase))
        {
            ApplyVehicleExamples(operation, method, route);
            return;
        }

        if (route.Contains("customers", StringComparison.OrdinalIgnoreCase))
        {
            ApplyCustomerExamples(operation, method, route);
            return;
        }

        if (route.Contains("bookings", StringComparison.OrdinalIgnoreCase))
        {
            ApplyBookingExamples(operation, method, route);
        }
    }

    private static void ApplyVehicleExamples(OpenApiOperation operation, string method, string route)
    {
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["registrationNumber"] = new OpenApiString("CA123456"),
                ["make"] = new OpenApiString("Toyota"),
                ["model"] = new OpenApiString("Corolla"),
                ["year"] = new OpenApiInteger(2022),
                ["dailyRate"] = new OpenApiDouble(450)
            });
            SetResponseExample(operation, 201, VehicleExample());
            return;
        }

        if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["registrationNumber"] = new OpenApiString("CA123456"),
                ["make"] = new OpenApiString("Toyota"),
                ["model"] = new OpenApiString("Corolla Quest"),
                ["year"] = new OpenApiInteger(2022),
                ["dailyRate"] = new OpenApiDouble(480)
            });
            SetResponseExample(operation, 200, VehicleExample());
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase) && route.Contains("{id}", StringComparison.Ordinal))
        {
            SetResponseExample(operation, 200, VehicleExample());
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            SetResponseExample(operation, 200, PagedVehicleExample());
        }
    }

    private static void ApplyCustomerExamples(OpenApiOperation operation, string method, string route)
    {
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["firstName"] = new OpenApiString("Thabo"),
                ["lastName"] = new OpenApiString("Mokoena"),
                ["email"] = new OpenApiString("thabo@example.com"),
                ["phoneNumber"] = new OpenApiString("+27821234567")
            });
            SetResponseExample(operation, 201, CustomerExample());
            return;
        }

        if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["firstName"] = new OpenApiString("Thabo"),
                ["lastName"] = new OpenApiString("Mokoena"),
                ["email"] = new OpenApiString("thabo.mokoena@example.com"),
                ["phoneNumber"] = new OpenApiString("+27821234567")
            });
            SetResponseExample(operation, 200, CustomerExample());
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase) && route.Contains("{id}", StringComparison.Ordinal))
        {
            SetResponseExample(operation, 200, CustomerExample());
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            SetResponseExample(operation, 200, PagedCustomerExample());
        }
    }

    private static void ApplyBookingExamples(OpenApiOperation operation, string method, string route)
    {
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["vehicleId"] = new OpenApiString(SampleVehicleId.ToString()),
                ["customerId"] = new OpenApiString(SampleCustomerId.ToString()),
                ["startDate"] = new OpenApiString("2026-10-01"),
                ["endDate"] = new OpenApiString("2026-10-03")
            });
            SetResponseExample(operation, 201, BookingExample());
            return;
        }

        if (method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["status"] = new OpenApiString("Completed")
            });
            SetResponseExample(operation, 200, BookingExample("Completed"));
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase) && route.Contains("{id}", StringComparison.Ordinal))
        {
            SetResponseExample(operation, 200, BookingExample());
            return;
        }

        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            SetResponseExample(operation, 200, PagedBookingExample());
        }
    }

    private static OpenApiObject VehicleExample() => new()
    {
        ["id"] = new OpenApiString(SampleVehicleId.ToString()),
        ["registrationNumber"] = new OpenApiString("CA123456"),
        ["make"] = new OpenApiString("Toyota"),
        ["model"] = new OpenApiString("Corolla"),
        ["year"] = new OpenApiInteger(2022),
        ["dailyRate"] = new OpenApiDouble(450),
        ["isDeleted"] = new OpenApiBoolean(false),
        ["createdDate"] = new OpenApiString("2026-08-01T10:00:00Z")
    };

    private static OpenApiObject CustomerExample() => new()
    {
        ["id"] = new OpenApiString(SampleCustomerId.ToString()),
        ["firstName"] = new OpenApiString("Thabo"),
        ["lastName"] = new OpenApiString("Mokoena"),
        ["email"] = new OpenApiString("thabo@example.com"),
        ["phoneNumber"] = new OpenApiString("+27821234567"),
        ["createdDate"] = new OpenApiString("2026-08-01T10:00:00Z")
    };

    private static OpenApiObject BookingExample(string status = "Active") => new()
    {
        ["id"] = new OpenApiString(SampleBookingId.ToString()),
        ["vehicleId"] = new OpenApiString(SampleVehicleId.ToString()),
        ["customerId"] = new OpenApiString(SampleCustomerId.ToString()),
        ["startDate"] = new OpenApiString("2026-10-01"),
        ["endDate"] = new OpenApiString("2026-10-03"),
        ["totalPrice"] = new OpenApiDouble(1350),
        ["status"] = new OpenApiString(status),
        ["createdDate"] = new OpenApiString("2026-08-15T09:30:00Z")
    };

    private static OpenApiObject PagedVehicleExample() => new()
    {
        ["items"] = new OpenApiArray { VehicleExample() },
        ["totalCount"] = new OpenApiInteger(1),
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalPages"] = new OpenApiInteger(1)
    };

    private static OpenApiObject PagedCustomerExample() => new()
    {
        ["items"] = new OpenApiArray { CustomerExample() },
        ["totalCount"] = new OpenApiInteger(1),
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalPages"] = new OpenApiInteger(1)
    };

    private static OpenApiObject PagedBookingExample() => new()
    {
        ["items"] = new OpenApiArray { BookingExample() },
        ["totalCount"] = new OpenApiInteger(1),
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalPages"] = new OpenApiInteger(1)
    };

    private static void SetRequestExample(OpenApiOperation operation, OpenApiObject example)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) == true)
        {
            mediaType.Example = example;
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, int statusCode, OpenApiObject example)
    {
        if (operation.Responses.TryGetValue(statusCode.ToString(), out var response) &&
            response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }
}
