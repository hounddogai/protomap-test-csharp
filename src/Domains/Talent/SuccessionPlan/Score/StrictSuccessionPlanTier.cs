namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict succession plan tier is in its life cycle.</summary>
public enum StrictSuccessionPlanTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a strict succession plan tier and its current status.</summary>
public sealed class StrictSuccessionPlanTier(string code, string name, double fte, int capacity, double budget, double ceiling, int createdAt, double cost, double minimum, StrictSuccessionPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int Capacity { get; set; } = capacity;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public int CreatedAt { get; set; } = createdAt;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public StrictSuccessionPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict succession plan tier in status draft.</summary>
    public static StrictSuccessionPlanTier NewStrictSuccessionPlanTier(string code, string name)
    {
        return new StrictSuccessionPlanTier(
            code,
            name,
            0.0,
            0,
            0.0,
            0.0,
            0,
            0.0,
            0.0,
            StrictSuccessionPlanTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSuccessionPlanTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSuccessionPlanTierStatus next)
    {
        switch (Status)
        {
            case StrictSuccessionPlanTierStatus.Draft:
                return next == StrictSuccessionPlanTierStatus.Pending || next == StrictSuccessionPlanTierStatus.Active;
            case StrictSuccessionPlanTierStatus.Pending:
                switch (next)
                {
                    case StrictSuccessionPlanTierStatus.Closed:
                        return true;
                }
                return false;
            case StrictSuccessionPlanTierStatus.Active:
                return next == StrictSuccessionPlanTierStatus.Closed;
            case StrictSuccessionPlanTierStatus.Suspended:
                return next == StrictSuccessionPlanTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictSuccessionPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict succession plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Budget - (double)Capacity, 2);
    }

    public StrictSuccessionPlanTier Copy()
    {
        return new StrictSuccessionPlanTier(
            Code,
            Name,
            Fte,
            Capacity,
            Budget,
            Ceiling,
            CreatedAt,
            Cost,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
