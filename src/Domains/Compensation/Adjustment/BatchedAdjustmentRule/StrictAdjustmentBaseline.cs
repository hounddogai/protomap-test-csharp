namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict adjustment baseline is in its life cycle.</summary>
public enum StrictAdjustmentBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict adjustment baseline, with what the planners need to act on it.</summary>
public sealed class StrictAdjustmentBaseline(string code, string name, int updatedAt, int version, int graceDays, string skill, StrictAdjustmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Version { get; set; } = version;
    public int GraceDays { get; set; } = graceDays;
    public string Skill { get; set; } = skill;
    public StrictAdjustmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a strict adjustment baseline in status requested.</summary>
    public static StrictAdjustmentBaseline NewStrictAdjustmentBaseline(string code, string name)
    {
        return new StrictAdjustmentBaseline(code, name, 0, 0, 0, "", StrictAdjustmentBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAdjustmentBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAdjustmentBaselineStatus next)
    {
        switch (Status)
        {
            case StrictAdjustmentBaselineStatus.Requested:
                switch (next)
                {
                    case StrictAdjustmentBaselineStatus.Granted:
                    case StrictAdjustmentBaselineStatus.Expired:
                        return true;
                }
                return false;
            case StrictAdjustmentBaselineStatus.Granted:
                return next == StrictAdjustmentBaselineStatus.Revoked;
            case StrictAdjustmentBaselineStatus.Revoked:
                return next == StrictAdjustmentBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictAdjustmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict adjustment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt - (double)Version, 2);
    }

    public StrictAdjustmentBaseline Copy()
    {
        return new StrictAdjustmentBaseline(Code, Name, UpdatedAt, Version, GraceDays, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
