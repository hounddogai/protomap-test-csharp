namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge mapping is in its life cycle.</summary>
public enum AccessBadgeMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One access badge mapping, with what the planners need to act on it.</summary>
public sealed class AccessBadgeMapping(string code, string name, string category, string label, int limit, double floorValue, string title, string summary, double ceiling, string currency, string country, AccessBadgeMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double FloorValue { get; set; } = floorValue;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Ceiling { get; set; } = ceiling;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public AccessBadgeMappingStatus Status { get; set; } = status;

    /// <summary>Starts a access badge mapping in status new.</summary>
    public static AccessBadgeMapping NewAccessBadgeMapping(string code, string name)
    {
        return new AccessBadgeMapping(code, name, "", "", 0, 0.0, "", "", 0.0, "", "", AccessBadgeMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeMappingStatus next)
    {
        switch (Status)
        {
            case AccessBadgeMappingStatus.New:
                switch (next)
                {
                    case AccessBadgeMappingStatus.Running:
                        return true;
                }
                return false;
            case AccessBadgeMappingStatus.Queued:
                switch (next)
                {
                    case AccessBadgeMappingStatus.Running:
                        return true;
                }
                return false;
            case AccessBadgeMappingStatus.Running:
                switch (next)
                {
                    case AccessBadgeMappingStatus.Done:
                    case AccessBadgeMappingStatus.Failed:
                        return true;
                }
                return false;
            case AccessBadgeMappingStatus.Done:
                return next == AccessBadgeMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"access badge mapping"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ceiling - FloorValue, 2);
    }

    public AccessBadgeMapping Copy()
    {
        return new AccessBadgeMapping(
            Code,
            Name,
            Category,
            Label,
            Limit,
            FloorValue,
            Title,
            Summary,
            Ceiling,
            Currency,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
