namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual scenario assignment is in its life cycle.</summary>
public enum ManualScenarioAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual scenario assignment as the scenario team keeps it.</summary>
public sealed class ManualScenarioAssignment(string code, string title, int priority, Dictionary<string, string> attributes, string timezone, ManualScenarioAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Timezone { get; set; } = timezone;
    public ManualScenarioAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual scenario assignment in status created.</summary>
    public static ManualScenarioAssignment NewManualScenarioAssignment(string code, string title)
    {
        return new ManualScenarioAssignment(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            "",
            ManualScenarioAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualScenarioAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualScenarioAssignmentStatus next)
    {
        switch (Status)
        {
            case ManualScenarioAssignmentStatus.Created:
                return next == ManualScenarioAssignmentStatus.Validated;
            case ManualScenarioAssignmentStatus.Validated:
                return next == ManualScenarioAssignmentStatus.Published || next == ManualScenarioAssignmentStatus.Retired;
            case ManualScenarioAssignmentStatus.Published:
                return next == ManualScenarioAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualScenarioAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual scenario assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public ManualScenarioAssignment Copy()
    {
        return new ManualScenarioAssignment(Code, Title, Priority, Attributes, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
