namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pool plan is in its life cycle.</summary>
public enum ScheduledPoolPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled pool plan. Its code is unique within a tenant.</summary>
public sealed class ScheduledPoolPlan(string code, string name, int retries, int startsAt, double hours, string currency, int tenureMonths, string timezone, string employeeId, double rate, ScheduledPoolPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int StartsAt { get; set; } = startsAt;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public ScheduledPoolPlanStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pool plan in status idle.</summary>
    public static ScheduledPoolPlan NewScheduledPoolPlan(string code, string name)
    {
        return new ScheduledPoolPlan(code, name, 0, 0, 0.0, "", 0, "", "", 0.0, ScheduledPoolPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPoolPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPoolPlanStatus next)
    {
        switch (Status)
        {
            case ScheduledPoolPlanStatus.Idle:
                switch (next)
                {
                    case ScheduledPoolPlanStatus.Warming:
                    case ScheduledPoolPlanStatus.Ready:
                        return true;
                }
                return false;
            case ScheduledPoolPlanStatus.Warming:
                switch (next)
                {
                    case ScheduledPoolPlanStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledPoolPlanStatus.Ready:
                return next == ScheduledPoolPlanStatus.Draining;
            case ScheduledPoolPlanStatus.Draining:
                return next == ScheduledPoolPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledPoolPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pool plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled pool plan"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rate - Hours, 2);
    }

    public ScheduledPoolPlan Copy()
    {
        return new ScheduledPoolPlan(
            Code,
            Name,
            Retries,
            StartsAt,
            Hours,
            Currency,
            TenureMonths,
            Timezone,
            EmployeeId,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
