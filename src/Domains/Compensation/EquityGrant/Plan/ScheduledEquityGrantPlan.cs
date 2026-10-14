namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled equity grant plan is in its life cycle.</summary>
public enum ScheduledEquityGrantPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled equity grant plan. Its code is unique within a tenant.</summary>
public sealed class ScheduledEquityGrantPlan(string code, string name, int expiresAt, int windowDays, bool active, string department, ScheduledEquityGrantPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public bool Active { get; set; } = active;
    public string Department { get; set; } = department;
    public ScheduledEquityGrantPlanStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled equity grant plan in status created.</summary>
    public static ScheduledEquityGrantPlan NewScheduledEquityGrantPlan(string code, string name)
    {
        return new ScheduledEquityGrantPlan(code, name, 0, 0, false, "", ScheduledEquityGrantPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEquityGrantPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEquityGrantPlanStatus next)
    {
        switch (Status)
        {
            case ScheduledEquityGrantPlanStatus.Created:
                switch (next)
                {
                    case ScheduledEquityGrantPlanStatus.Published:
                    case ScheduledEquityGrantPlanStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledEquityGrantPlanStatus.Validated:
                return next == ScheduledEquityGrantPlanStatus.Published;
            case ScheduledEquityGrantPlanStatus.Published:
                switch (next)
                {
                    case ScheduledEquityGrantPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledEquityGrantPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled equity grant plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)WindowDays - (double)ExpiresAt, 2);
    }

    public ScheduledEquityGrantPlan Copy()
    {
        return new ScheduledEquityGrantPlan(Code, Name, ExpiresAt, WindowDays, Active, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
