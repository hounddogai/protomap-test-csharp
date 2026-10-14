namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global adjustment rollup is in its life cycle.</summary>
public enum GlobalAdjustmentRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One global adjustment rollup, with what the planners need to act on it.</summary>
public sealed class GlobalAdjustmentRollup(string code, string name, double threshold, double amount, double hours, string salaryBand, string checksum, string timezone, Dictionary<string, string> attributes, double discount, int createdAt, GlobalAdjustmentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public int CreatedAt { get; set; } = createdAt;
    public GlobalAdjustmentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global adjustment rollup in status planned.</summary>
    public static GlobalAdjustmentRollup NewGlobalAdjustmentRollup(string code, string name)
    {
        return new GlobalAdjustmentRollup(
            code,
            name,
            0.0,
            0.0,
            0.0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            GlobalAdjustmentRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAdjustmentRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAdjustmentRollupStatus next)
    {
        switch (Status)
        {
            case GlobalAdjustmentRollupStatus.Planned:
                switch (next)
                {
                    case GlobalAdjustmentRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalAdjustmentRollupStatus.Scheduled:
                return next == GlobalAdjustmentRollupStatus.InProgress || next == GlobalAdjustmentRollupStatus.Completed;
            case GlobalAdjustmentRollupStatus.InProgress:
                return next == GlobalAdjustmentRollupStatus.Completed || next == GlobalAdjustmentRollupStatus.Cancelled;
            case GlobalAdjustmentRollupStatus.Completed:
                switch (next)
                {
                    case GlobalAdjustmentRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAdjustmentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global adjustment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global adjustment rollup"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round(Discount * Threshold, 2);
    }

    public GlobalAdjustmentRollup Copy()
    {
        return new GlobalAdjustmentRollup(
            Code,
            Name,
            Threshold,
            Amount,
            Hours,
            SalaryBand,
            Checksum,
            Timezone,
            Attributes,
            Discount,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
