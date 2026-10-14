namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario snapshot is in its life cycle.</summary>
public enum ScenarioSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scenario snapshot. Its code is unique within a tenant.</summary>
public sealed class ScenarioSnapshot(string code, string name, string label, string badge, double margin, string grade, int durationMinutes, string skill, string department, double threshold, double score, ScenarioSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Skill { get; set; } = skill;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public double Score { get; set; } = score;
    public ScenarioSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scenario snapshot in status draft.</summary>
    public static ScenarioSnapshot NewScenarioSnapshot(string code, string name)
    {
        return new ScenarioSnapshot(code, name, "", "", 0.0, "", 0, "", "", 0.0, 0.0, ScenarioSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioSnapshotStatus next)
    {
        switch (Status)
        {
            case ScenarioSnapshotStatus.Draft:
                switch (next)
                {
                    case ScenarioSnapshotStatus.Active:
                    case ScenarioSnapshotStatus.Closed:
                        return true;
                }
                return false;
            case ScenarioSnapshotStatus.Pending:
                switch (next)
                {
                    case ScenarioSnapshotStatus.Active:
                    case ScenarioSnapshotStatus.Suspended:
                        return true;
                }
                return false;
            case ScenarioSnapshotStatus.Active:
                return next == ScenarioSnapshotStatus.Suspended;
            case ScenarioSnapshotStatus.Suspended:
                return next == ScenarioSnapshotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScenarioSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario snapshot"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Threshold - Score, 2);
    }

    public ScenarioSnapshot Copy()
    {
        return new ScenarioSnapshot(
            Code,
            Name,
            Label,
            Badge,
            Margin,
            Grade,
            DurationMinutes,
            Skill,
            Department,
            Threshold,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
