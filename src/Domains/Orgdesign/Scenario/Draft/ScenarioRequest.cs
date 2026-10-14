namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario request is in its life cycle.</summary>
public enum ScenarioRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scenario request. Its code is unique within a tenant.</summary>
public sealed class ScenarioRequest(string code, string label, int durationMinutes, double weight, int version, string jobCode, ScenarioRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Weight { get; set; } = weight;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public ScenarioRequestStatus Status { get; set; } = status;

    /// <summary>Starts a scenario request in status draft.</summary>
    public static ScenarioRequest NewScenarioRequest(string code, string label)
    {
        return new ScenarioRequest(code, label, 0, 0.0, 0, "", ScenarioRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioRequestStatus next)
    {
        switch (Status)
        {
            case ScenarioRequestStatus.Draft:
                switch (next)
                {
                    case ScenarioRequestStatus.Suspended:
                        return true;
                }
                return false;
            case ScenarioRequestStatus.Pending:
                return next == ScenarioRequestStatus.Suspended || next == ScenarioRequestStatus.Closed;
            case ScenarioRequestStatus.Active:
                return next == ScenarioRequestStatus.Suspended || next == ScenarioRequestStatus.Closed;
            case ScenarioRequestStatus.Suspended:
                return next == ScenarioRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScenarioRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Version - (double)DurationMinutes, 2);
    }

    public ScenarioRequest Copy()
    {
        return new ScenarioRequest(Code, Label, DurationMinutes, Weight, Version, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
