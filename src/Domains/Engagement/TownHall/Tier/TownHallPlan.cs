namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall plan is in its life cycle.</summary>
public enum TownHallPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A town hall plan as the town hall team keeps it.</summary>
public sealed class TownHallPlan(string code, string name, string unit, double maximum, bool archived, int endsAt, string externalRef, int durationMinutes, int capacity, bool approved, TownHallPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public bool Approved { get; set; } = approved;
    public TownHallPlanStatus Status { get; set; } = status;

    /// <summary>Starts a town hall plan in status open.</summary>
    public static TownHallPlan NewTownHallPlan(string code, string name)
    {
        return new TownHallPlan(code, name, "", 0.0, false, 0, "", 0, 0, false, TownHallPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallPlanStatus next)
    {
        switch (Status)
        {
            case TownHallPlanStatus.Open:
                switch (next)
                {
                    case TownHallPlanStatus.InReview:
                        return true;
                }
                return false;
            case TownHallPlanStatus.InReview:
                switch (next)
                {
                    case TownHallPlanStatus.Reopened:
                        return true;
                }
                return false;
            case TownHallPlanStatus.Resolved:
                return next == TownHallPlanStatus.Reopened || next == TownHallPlanStatus.Archived;
            case TownHallPlanStatus.Reopened:
                switch (next)
                {
                    case TownHallPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)EndsAt + (double)Capacity, 2);
    }

    public TownHallPlan Copy()
    {
        return new TownHallPlan(
            Code,
            Name,
            Unit,
            Maximum,
            Archived,
            EndsAt,
            ExternalRef,
            DurationMinutes,
            Capacity,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
