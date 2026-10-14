namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario mapping is in its life cycle.</summary>
public enum ScenarioMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scenario mapping. Its code is unique within a tenant.</summary>
public sealed class ScenarioMapping(string code, string name, int graceDays, bool archived, bool active, ScenarioMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public bool Archived { get; set; } = archived;
    public bool Active { get; set; } = active;
    public ScenarioMappingStatus Status { get; set; } = status;

    /// <summary>Starts a scenario mapping in status created.</summary>
    public static ScenarioMapping NewScenarioMapping(string code, string name)
    {
        return new ScenarioMapping(code, name, 0, false, false, ScenarioMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioMappingStatus next)
    {
        switch (Status)
        {
            case ScenarioMappingStatus.Created:
                return next == ScenarioMappingStatus.Published || next == ScenarioMappingStatus.Retired;
            case ScenarioMappingStatus.Validated:
                return next == ScenarioMappingStatus.Published || next == ScenarioMappingStatus.Retired;
            case ScenarioMappingStatus.Published:
                return next == ScenarioMappingStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScenarioMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public ScenarioMapping Copy()
    {
        return new ScenarioMapping(Code, Name, GraceDays, Archived, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
