namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted warranty mapping is in its life cycle.</summary>
public enum WeightedWarrantyMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One weighted warranty mapping, with what the planners need to act on it.</summary>
public sealed class WeightedWarrantyMapping(string code, string label, int priority, List<string> tags, int limit, string timezone, int windowDays, string phone, string jobCode, WeightedWarrantyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public string JobCode { get; set; } = jobCode;
    public WeightedWarrantyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a weighted warranty mapping in status proposed.</summary>
    public static WeightedWarrantyMapping NewWeightedWarrantyMapping(string code, string label)
    {
        return new WeightedWarrantyMapping(
            code,
            label,
            0,
            new List<string>(),
            0,
            "",
            0,
            "",
            "",
            WeightedWarrantyMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedWarrantyMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedWarrantyMappingStatus next)
    {
        switch (Status)
        {
            case WeightedWarrantyMappingStatus.Proposed:
                switch (next)
                {
                    case WeightedWarrantyMappingStatus.Rejected:
                    case WeightedWarrantyMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedWarrantyMappingStatus.Approved:
                switch (next)
                {
                    case WeightedWarrantyMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedWarrantyMappingStatus.Rejected:
                return next == WeightedWarrantyMappingStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedWarrantyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted warranty mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round((double)WindowDays * (double)Limit, 2);
    }

    public WeightedWarrantyMapping Copy()
    {
        return new WeightedWarrantyMapping(
            Code,
            Label,
            Priority,
            Tags,
            Limit,
            Timezone,
            WindowDays,
            Phone,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
