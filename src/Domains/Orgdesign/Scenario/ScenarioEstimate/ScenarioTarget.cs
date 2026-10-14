namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario target is in its life cycle.</summary>
public enum ScenarioTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scenario target, with what the planners need to act on it.</summary>
public sealed class ScenarioTarget(string code, string name, string employeeId, int createdAt, int windowDays, ScenarioTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public ScenarioTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scenario target in status proposed.</summary>
    public static ScenarioTarget NewScenarioTarget(string code, string name)
    {
        return new ScenarioTarget(code, name, "", 0, 0, ScenarioTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioTargetStatus next)
    {
        switch (Status)
        {
            case ScenarioTargetStatus.Proposed:
                switch (next)
                {
                    case ScenarioTargetStatus.Approved:
                    case ScenarioTargetStatus.Rejected:
                        return true;
                }
                return false;
            case ScenarioTargetStatus.Approved:
                return next == ScenarioTargetStatus.Rejected;
            case ScenarioTargetStatus.Rejected:
                return next == ScenarioTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScenarioTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays + (double)CreatedAt, 2);
    }

    public ScenarioTarget Copy()
    {
        return new ScenarioTarget(Code, Name, EmployeeId, CreatedAt, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
