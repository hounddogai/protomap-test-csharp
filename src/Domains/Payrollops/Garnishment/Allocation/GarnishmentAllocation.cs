namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment allocation is in its life cycle.</summary>
public enum GarnishmentAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One garnishment allocation, with what the planners need to act on it.</summary>
public sealed class GarnishmentAllocation(string code, string title, string contactEmail, string externalRef, string region, int quantity, GarnishmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string ExternalRef { get; set; } = externalRef;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public GarnishmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment allocation in status created.</summary>
    public static GarnishmentAllocation NewGarnishmentAllocation(string code, string title)
    {
        return new GarnishmentAllocation(code, title, "", "", "", 0, GarnishmentAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentAllocationStatus next)
    {
        switch (Status)
        {
            case GarnishmentAllocationStatus.Created:
                return next == GarnishmentAllocationStatus.Published || next == GarnishmentAllocationStatus.Retired;
            case GarnishmentAllocationStatus.Validated:
                return next == GarnishmentAllocationStatus.Published || next == GarnishmentAllocationStatus.Retired;
            case GarnishmentAllocationStatus.Published:
                return next == GarnishmentAllocationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GarnishmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public GarnishmentAllocation Copy()
    {
        return new GarnishmentAllocation(Code, Title, ContactEmail, ExternalRef, Region, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
