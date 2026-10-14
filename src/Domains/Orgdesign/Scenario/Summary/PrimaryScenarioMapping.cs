namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary scenario mapping is in its life cycle.</summary>
public enum PrimaryScenarioMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One primary scenario mapping, with what the planners need to act on it.</summary>
public sealed class PrimaryScenarioMapping(string code, string label, int updatedAt, string unit, double budget, string source, string department, int endsAt, PrimaryScenarioMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Unit { get; set; } = unit;
    public double Budget { get; set; } = budget;
    public string Source { get; set; } = source;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public PrimaryScenarioMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary scenario mapping in status planned.</summary>
    public static PrimaryScenarioMapping NewPrimaryScenarioMapping(string code, string label)
    {
        return new PrimaryScenarioMapping(code, label, 0, "", 0.0, "", "", 0, PrimaryScenarioMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryScenarioMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryScenarioMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryScenarioMappingStatus.Planned:
                return next == PrimaryScenarioMappingStatus.Scheduled || next == PrimaryScenarioMappingStatus.InProgress;
            case PrimaryScenarioMappingStatus.Scheduled:
                return next == PrimaryScenarioMappingStatus.Completed;
            case PrimaryScenarioMappingStatus.InProgress:
                switch (next)
                {
                    case PrimaryScenarioMappingStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryScenarioMappingStatus.Completed:
                switch (next)
                {
                    case PrimaryScenarioMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryScenarioMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary scenario mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary scenario mapping"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Budget + (double)UpdatedAt, 2);
    }

    public PrimaryScenarioMapping Copy()
    {
        return new PrimaryScenarioMapping(Code, Label, UpdatedAt, Unit, Budget, Source, Department, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
