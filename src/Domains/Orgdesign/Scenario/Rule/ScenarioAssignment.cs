namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario assignment is in its life cycle.</summary>
public enum ScenarioAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scenario assignment, with what the planners need to act on it.</summary>
public sealed class ScenarioAssignment(string code, string label, double ratio, double minimum, double ceiling, List<string> tags, double floorValue, ScenarioAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public double Minimum { get; set; } = minimum;
    public double Ceiling { get; set; } = ceiling;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public ScenarioAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a scenario assignment in status created.</summary>
    public static ScenarioAssignment NewScenarioAssignment(string code, string label)
    {
        return new ScenarioAssignment(
            code,
            label,
            0.0,
            0.0,
            0.0,
            new List<string>(),
            0.0,
            ScenarioAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioAssignmentStatus next)
    {
        switch (Status)
        {
            case ScenarioAssignmentStatus.Created:
                switch (next)
                {
                    case ScenarioAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case ScenarioAssignmentStatus.Validated:
                return next == ScenarioAssignmentStatus.Published;
            case ScenarioAssignmentStatus.Published:
                return next == ScenarioAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScenarioAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling + Ratio, 2);
    }

    public ScenarioAssignment Copy()
    {
        return new ScenarioAssignment(Code, Label, Ratio, Minimum, Ceiling, Tags, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
