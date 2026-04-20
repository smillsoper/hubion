using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Services;
using Hubion.Domain.Entities;

namespace Hubion.Api.Endpoints;

public static class CallRecordsEndpoints
{
    public static IEndpointRouteBuilder MapCallRecordsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/call-records").RequireAuthorization();

        group.MapGet("{id:guid}", GetById);

        return app;
    }

    private static async Task<IResult> GetById(
        Guid id,
        ICallRecordRepository callRecords,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var record = await callRecords.GetByIdWithInteractionsAsync(id, ct);

        return record is null ? Results.NotFound() : Results.Ok(ToResponse(record));
    }

    private static object ToResponse(CallRecord r) => new
    {
        r.Id,
        r.TenantId,
        r.ClientId,
        r.CampaignId,
        r.AgentId,
        r.Source,
        r.RecordType,
        r.OverallStatus,
        CallerIdentity = new
        {
            r.CallerId,
            r.AccountNumber,
            r.FirstName,
            r.LastName,
            r.Email,
            r.Phone
        },
        Timing = new
        {
            r.CallStartAt,
            r.CallEndAt,
            r.HandleTimeSeconds
        },
        Financial = new
        {
            r.TotalAmount,
            r.TaxAmount,
            r.PaymentStatus
        },
        Fulfillment = new
        {
            r.FulfillmentStatus,
            r.TrackingNumber
        },
        r.Addresses,
        r.CommitmentEvents,
        r.RecordingUrl,
        Interactions = r.Interactions.Select(i => new
        {
            i.Id,
            i.InteractionNumber,
            i.Type,
            i.FlowId,
            i.FlowVersion,
            i.Disposition,
            i.Status,
            i.CommitmentEvents,
            i.CartId,
            i.StartedAt,
            i.CompletedAt
        }),
        r.CreatedAt,
        r.UpdatedAt
    };
}
