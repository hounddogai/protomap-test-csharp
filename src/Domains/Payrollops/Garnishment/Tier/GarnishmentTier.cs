namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment tier is in its life cycle.</summary>
public enum GarnishmentTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a garnishment tier and its current status.</summary>
public sealed class GarnishmentTier(string code, string name, string unit, int attempts, string employeeId, string contactEmail, string region, bool pinned, string badge, string category, GarnishmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public GarnishmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment tier in status planned.</summary>
    public static GarnishmentTier NewGarnishmentTier(string code, string name)
    {
        return new GarnishmentTier(code, name, "", 0, "", "", "", false, "", "", GarnishmentTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentTierStatus next)
    {
        switch (Status)
        {
            case GarnishmentTierStatus.Planned:
                return next == GarnishmentTierStatus.InProgress;
            case GarnishmentTierStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentTierStatus.InProgress:
                        return true;
                }
                return false;
            case GarnishmentTierStatus.InProgress:
                return next == GarnishmentTierStatus.Cancelled;
            case GarnishmentTierStatus.Completed:
                return next == GarnishmentTierStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GarnishmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public GarnishmentTier Copy()
    {
        return new GarnishmentTier(
            Code,
            Name,
            Unit,
            Attempts,
            EmployeeId,
            ContactEmail,
            Region,
            Pinned,
            Badge,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
