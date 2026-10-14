namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted scenario mapping is in its life cycle.</summary>
public enum WeightedScenarioMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One weighted scenario mapping, with what the planners need to act on it.</summary>
public sealed class WeightedScenarioMapping(string code, string title, int leaveDays, double ratio, string department, string unit, bool approved, List<string> tags, double threshold, int capacity, string label, WeightedScenarioMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public WeightedScenarioMappingStatus Status { get; set; } = status;

    /// <summary>Starts a weighted scenario mapping in status planned.</summary>
    public static WeightedScenarioMapping NewWeightedScenarioMapping(string code, string title)
    {
        return new WeightedScenarioMapping(
            code,
            title,
            0,
            0.0,
            "",
            "",
            false,
            new List<string>(),
            0.0,
            0,
            "",
            WeightedScenarioMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedScenarioMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedScenarioMappingStatus next)
    {
        switch (Status)
        {
            case WeightedScenarioMappingStatus.Planned:
                return next == WeightedScenarioMappingStatus.Scheduled || next == WeightedScenarioMappingStatus.Completed;
            case WeightedScenarioMappingStatus.Scheduled:
                return next == WeightedScenarioMappingStatus.Cancelled;
            case WeightedScenarioMappingStatus.InProgress:
                switch (next)
                {
                    case WeightedScenarioMappingStatus.Completed:
                    case WeightedScenarioMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedScenarioMappingStatus.Completed:
                switch (next)
                {
                    case WeightedScenarioMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedScenarioMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted scenario mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
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

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity + Ratio, 2);
    }

    public WeightedScenarioMapping Copy()
    {
        return new WeightedScenarioMapping(
            Code,
            Title,
            LeaveDays,
            Ratio,
            Department,
            Unit,
            Approved,
            Tags,
            Threshold,
            Capacity,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
