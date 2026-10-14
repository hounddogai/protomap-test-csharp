namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default town hall plan is in its life cycle.</summary>
public enum DefaultTownHallPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default town hall plan and its current status.</summary>
public sealed class DefaultTownHallPlan(string code, string name, double ratio, string country, int updatedAt, DefaultTownHallPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public DefaultTownHallPlanStatus Status { get; set; } = status;

    /// <summary>Starts a default town hall plan in status planned.</summary>
    public static DefaultTownHallPlan NewDefaultTownHallPlan(string code, string name)
    {
        return new DefaultTownHallPlan(code, name, 0.0, "", 0, DefaultTownHallPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultTownHallPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultTownHallPlanStatus next)
    {
        switch (Status)
        {
            case DefaultTownHallPlanStatus.Planned:
                return next == DefaultTownHallPlanStatus.Completed || next == DefaultTownHallPlanStatus.Cancelled;
            case DefaultTownHallPlanStatus.Scheduled:
                return next == DefaultTownHallPlanStatus.InProgress || next == DefaultTownHallPlanStatus.Cancelled;
            case DefaultTownHallPlanStatus.InProgress:
                return next == DefaultTownHallPlanStatus.Completed || next == DefaultTownHallPlanStatus.Cancelled;
            case DefaultTownHallPlanStatus.Completed:
                return next == DefaultTownHallPlanStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultTownHallPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default town hall plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default town hall plan"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)UpdatedAt - Ratio, 2);
    }

    public DefaultTownHallPlan Copy()
    {
        return new DefaultTownHallPlan(Code, Name, Ratio, Country, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
