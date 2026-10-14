namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario milestone is in its life cycle.</summary>
public enum ScenarioMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scenario milestone, with what the planners need to act on it.</summary>
public sealed class ScenarioMilestone(string code, string title, int priority, int version, int batchSize, int graceDays, int durationMinutes, Dictionary<string, string> attributes, string location, double amount, ScenarioMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public int GraceDays { get; set; } = graceDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public ScenarioMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a scenario milestone in status idle.</summary>
    public static ScenarioMilestone NewScenarioMilestone(string code, string title)
    {
        return new ScenarioMilestone(
            code,
            title,
            0,
            0,
            0,
            0,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            ScenarioMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioMilestoneStatus next)
    {
        switch (Status)
        {
            case ScenarioMilestoneStatus.Idle:
                return next == ScenarioMilestoneStatus.Ready || next == ScenarioMilestoneStatus.Draining;
            case ScenarioMilestoneStatus.Warming:
                switch (next)
                {
                    case ScenarioMilestoneStatus.Ready:
                    case ScenarioMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioMilestoneStatus.Ready:
                switch (next)
                {
                    case ScenarioMilestoneStatus.Draining:
                    case ScenarioMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioMilestoneStatus.Draining:
                switch (next)
                {
                    case ScenarioMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return Math.Round(Amount + (double)Priority, 2);
    }

    public ScenarioMilestone Copy()
    {
        return new ScenarioMilestone(
            Code,
            Title,
            Priority,
            Version,
            BatchSize,
            GraceDays,
            DurationMinutes,
            Attributes,
            Location,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
