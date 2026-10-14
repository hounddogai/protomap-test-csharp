namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool tier is in its life cycle.</summary>
public enum BonusPoolTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a bonus pool tier and its current status.</summary>
public sealed class BonusPoolTier(string code, string label, double hours, int expiresAt, int updatedAt, string location, int retries, double cost, double ceiling, BonusPoolTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public double Ceiling { get; set; } = ceiling;
    public BonusPoolTierStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool tier in status planned.</summary>
    public static BonusPoolTier NewBonusPoolTier(string code, string label)
    {
        return new BonusPoolTier(code, label, 0.0, 0, 0, "", 0, 0.0, 0.0, BonusPoolTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolTierStatus next)
    {
        switch (Status)
        {
            case BonusPoolTierStatus.Planned:
                switch (next)
                {
                    case BonusPoolTierStatus.Completed:
                    case BonusPoolTierStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolTierStatus.Scheduled:
                return next == BonusPoolTierStatus.Cancelled;
            case BonusPoolTierStatus.InProgress:
                return next == BonusPoolTierStatus.Completed || next == BonusPoolTierStatus.Cancelled;
            case BonusPoolTierStatus.Completed:
                return next == BonusPoolTierStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BonusPoolTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool tier"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt - Ceiling, 2);
    }

    public BonusPoolTier Copy()
    {
        return new BonusPoolTier(Code, Label, Hours, ExpiresAt, UpdatedAt, Location, Retries, Cost, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
