namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool estimate is in its life cycle.</summary>
public enum BonusPoolEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A bonus pool estimate as the bonus pool team keeps it.</summary>
public sealed class BonusPoolEstimate(string code, string name, Dictionary<string, string> attributes, int endsAt, string timezone, double maximum, BonusPoolEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int EndsAt { get; set; } = endsAt;
    public string Timezone { get; set; } = timezone;
    public double Maximum { get; set; } = maximum;
    public BonusPoolEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool estimate in status proposed.</summary>
    public static BonusPoolEstimate NewBonusPoolEstimate(string code, string name)
    {
        return new BonusPoolEstimate(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            BonusPoolEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolEstimateStatus next)
    {
        switch (Status)
        {
            case BonusPoolEstimateStatus.Proposed:
                switch (next)
                {
                    case BonusPoolEstimateStatus.Approved:
                        return true;
                }
                return false;
            case BonusPoolEstimateStatus.Approved:
                switch (next)
                {
                    case BonusPoolEstimateStatus.Rejected:
                    case BonusPoolEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case BonusPoolEstimateStatus.Rejected:
                switch (next)
                {
                    case BonusPoolEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round(Maximum * (double)EndsAt, 2);
    }

    public BonusPoolEstimate Copy()
    {
        return new BonusPoolEstimate(Code, Name, Attributes, EndsAt, Timezone, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
