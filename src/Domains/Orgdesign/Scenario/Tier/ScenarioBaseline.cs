namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario baseline is in its life cycle.</summary>
public enum ScenarioBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scenario baseline as the scenario team keeps it.</summary>
public sealed class ScenarioBaseline(string code, string label, int revision, string unit, int version, string title, List<string> tags, double cost, int expiresAt, string reviewer, ScenarioBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public int Version { get; set; } = version;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public ScenarioBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scenario baseline in status created.</summary>
    public static ScenarioBaseline NewScenarioBaseline(string code, string label)
    {
        return new ScenarioBaseline(
            code,
            label,
            0,
            "",
            0,
            "",
            new List<string>(),
            0.0,
            0,
            "",
            ScenarioBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioBaselineStatus next)
    {
        switch (Status)
        {
            case ScenarioBaselineStatus.Created:
                switch (next)
                {
                    case ScenarioBaselineStatus.Validated:
                    case ScenarioBaselineStatus.Published:
                        return true;
                }
                return false;
            case ScenarioBaselineStatus.Validated:
                return next == ScenarioBaselineStatus.Published || next == ScenarioBaselineStatus.Retired;
            case ScenarioBaselineStatus.Published:
                return next == ScenarioBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScenarioBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double ProjectedTotal()
    {
        return Math.Round((double)Version * (double)Revision, 2);
    }

    public ScenarioBaseline Copy()
    {
        return new ScenarioBaseline(
            Code,
            Label,
            Revision,
            Unit,
            Version,
            Title,
            Tags,
            Cost,
            ExpiresAt,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
