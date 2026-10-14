namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount cycle is in its life cycle.</summary>
public enum HeadcountCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A headcount cycle as the headcount team keeps it.</summary>
public sealed class HeadcountCycle(string code, string label, int windowDays, int capacity, double maximum, double cost, int version, Dictionary<string, string> attributes, string badge, int priority, string managerId, HeadcountCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public int Priority { get; set; } = priority;
    public string ManagerId { get; set; } = managerId;
    public HeadcountCycleStatus Status { get; set; } = status;

    /// <summary>Starts a headcount cycle in status open.</summary>
    public static HeadcountCycle NewHeadcountCycle(string code, string label)
    {
        return new HeadcountCycle(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            HeadcountCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountCycleStatus next)
    {
        switch (Status)
        {
            case HeadcountCycleStatus.Open:
                return next == HeadcountCycleStatus.InReview || next == HeadcountCycleStatus.Reopened;
            case HeadcountCycleStatus.InReview:
                return next == HeadcountCycleStatus.Reopened;
            case HeadcountCycleStatus.Resolved:
                return next == HeadcountCycleStatus.Archived;
            case HeadcountCycleStatus.Reopened:
                switch (next)
                {
                    case HeadcountCycleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"headcount cycle"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Cost - (double)Priority, 2);
    }

    public HeadcountCycle Copy()
    {
        return new HeadcountCycle(
            Code,
            Label,
            WindowDays,
            Capacity,
            Maximum,
            Cost,
            Version,
            Attributes,
            Badge,
            Priority,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
