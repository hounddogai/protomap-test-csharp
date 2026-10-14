namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending equity grant plan is in its life cycle.</summary>
public enum PendingEquityGrantPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending equity grant plan. Its code is unique within a tenant.</summary>
public sealed class PendingEquityGrantPlan(string code, string name, string region, string notes, string location, bool approved, int limit, string contactEmail, string department, int windowDays, PendingEquityGrantPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public bool Approved { get; set; } = approved;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public string Department { get; set; } = department;
    public int WindowDays { get; set; } = windowDays;
    public PendingEquityGrantPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending equity grant plan in status planned.</summary>
    public static PendingEquityGrantPlan NewPendingEquityGrantPlan(string code, string name)
    {
        return new PendingEquityGrantPlan(
            code,
            name,
            "",
            "",
            "",
            false,
            0,
            "",
            "",
            0,
            PendingEquityGrantPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingEquityGrantPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingEquityGrantPlanStatus next)
    {
        switch (Status)
        {
            case PendingEquityGrantPlanStatus.Planned:
                switch (next)
                {
                    case PendingEquityGrantPlanStatus.InProgress:
                    case PendingEquityGrantPlanStatus.Completed:
                        return true;
                }
                return false;
            case PendingEquityGrantPlanStatus.Scheduled:
                return next == PendingEquityGrantPlanStatus.Completed || next == PendingEquityGrantPlanStatus.Cancelled;
            case PendingEquityGrantPlanStatus.InProgress:
                switch (next)
                {
                    case PendingEquityGrantPlanStatus.Completed:
                        return true;
                }
                return false;
            case PendingEquityGrantPlanStatus.Completed:
                return next == PendingEquityGrantPlanStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingEquityGrantPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending equity grant plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending equity grant plan"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Limit * (double)WindowDays, 2);
    }

    public PendingEquityGrantPlan Copy()
    {
        return new PendingEquityGrantPlan(
            Code,
            Name,
            Region,
            Notes,
            Location,
            Approved,
            Limit,
            ContactEmail,
            Department,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
