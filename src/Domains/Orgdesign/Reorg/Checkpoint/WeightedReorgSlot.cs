namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted reorg slot is in its life cycle.</summary>
public enum WeightedReorgSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted reorg slot, with what the planners need to act on it.</summary>
public sealed class WeightedReorgSlot(string code, string label, string currency, int version, Dictionary<string, string> attributes, double cost, string summary, string checksum, string salaryBand, WeightedReorgSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public string SalaryBand { get; set; } = salaryBand;
    public WeightedReorgSlotStatus Status { get; set; } = status;

    /// <summary>Starts a weighted reorg slot in status open.</summary>
    public static WeightedReorgSlot NewWeightedReorgSlot(string code, string label)
    {
        return new WeightedReorgSlot(
            code,
            label,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            WeightedReorgSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReorgSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReorgSlotStatus next)
    {
        switch (Status)
        {
            case WeightedReorgSlotStatus.Open:
                switch (next)
                {
                    case WeightedReorgSlotStatus.InReview:
                    case WeightedReorgSlotStatus.Reopened:
                        return true;
                }
                return false;
            case WeightedReorgSlotStatus.InReview:
                return next == WeightedReorgSlotStatus.Resolved || next == WeightedReorgSlotStatus.Reopened;
            case WeightedReorgSlotStatus.Resolved:
                return next == WeightedReorgSlotStatus.Archived;
            case WeightedReorgSlotStatus.Reopened:
                switch (next)
                {
                    case WeightedReorgSlotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReorgSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted reorg slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double EffectiveExposure()
    {
        return Math.Round(Cost * (double)Version, 2);
    }

    public WeightedReorgSlot Copy()
    {
        return new WeightedReorgSlot(
            Code,
            Label,
            Currency,
            Version,
            Attributes,
            Cost,
            Summary,
            Checksum,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
