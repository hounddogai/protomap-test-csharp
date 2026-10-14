namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled occupancy plan is in its life cycle.</summary>
public enum ScheduledOccupancyPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled occupancy plan as the occupancy team keeps it.</summary>
public sealed class ScheduledOccupancyPlan(string code, string name, string locale, int startsAt, int expiresAt, ScheduledOccupancyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public int StartsAt { get; set; } = startsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledOccupancyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled occupancy plan in status open.</summary>
    public static ScheduledOccupancyPlan NewScheduledOccupancyPlan(string code, string name)
    {
        return new ScheduledOccupancyPlan(code, name, "", 0, 0, ScheduledOccupancyPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledOccupancyPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledOccupancyPlanStatus next)
    {
        switch (Status)
        {
            case ScheduledOccupancyPlanStatus.Open:
                switch (next)
                {
                    case ScheduledOccupancyPlanStatus.InReview:
                    case ScheduledOccupancyPlanStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledOccupancyPlanStatus.InReview:
                return next == ScheduledOccupancyPlanStatus.Resolved;
            case ScheduledOccupancyPlanStatus.Resolved:
                return next == ScheduledOccupancyPlanStatus.Archived;
            case ScheduledOccupancyPlanStatus.Reopened:
                switch (next)
                {
                    case ScheduledOccupancyPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledOccupancyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled occupancy plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)StartsAt + (double)ExpiresAt, 2);
    }

    public ScheduledOccupancyPlan Copy()
    {
        return new ScheduledOccupancyPlan(Code, Name, Locale, StartsAt, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
