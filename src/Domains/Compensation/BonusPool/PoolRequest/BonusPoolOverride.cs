namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool override is in its life cycle.</summary>
public enum BonusPoolOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a bonus pool override and its current status.</summary>
public sealed class BonusPoolOverride(string code, string label, string category, int updatedAt, string description, int durationMinutes, double minimum, int revision, BonusPoolOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Description { get; set; } = description;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Minimum { get; set; } = minimum;
    public int Revision { get; set; } = revision;
    public BonusPoolOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool override in status requested.</summary>
    public static BonusPoolOverride NewBonusPoolOverride(string code, string label)
    {
        return new BonusPoolOverride(code, label, "", 0, "", 0, 0.0, 0, BonusPoolOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolOverrideStatus next)
    {
        switch (Status)
        {
            case BonusPoolOverrideStatus.Requested:
                switch (next)
                {
                    case BonusPoolOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case BonusPoolOverrideStatus.Granted:
                switch (next)
                {
                    case BonusPoolOverrideStatus.Revoked:
                    case BonusPoolOverrideStatus.Expired:
                        return true;
                }
                return false;
            case BonusPoolOverrideStatus.Revoked:
                switch (next)
                {
                    case BonusPoolOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool override"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Revision + (double)DurationMinutes, 2);
    }

    public BonusPoolOverride Copy()
    {
        return new BonusPoolOverride(
            Code,
            Label,
            Category,
            UpdatedAt,
            Description,
            DurationMinutes,
            Minimum,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
