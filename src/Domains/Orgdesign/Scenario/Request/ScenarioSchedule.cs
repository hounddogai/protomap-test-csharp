namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario schedule is in its life cycle.</summary>
public enum ScenarioScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scenario schedule and its current status.</summary>
public sealed class ScenarioSchedule(string code, string name, int version, int batchSize, string externalRef, string unit, double discount, string summary, int priority, double rating, ScenarioScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public double Rating { get; set; } = rating;
    public ScenarioScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a scenario schedule in status draft.</summary>
    public static ScenarioSchedule NewScenarioSchedule(string code, string name)
    {
        return new ScenarioSchedule(code, name, 0, 0, "", "", 0.0, "", 0, 0.0, ScenarioScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioScheduleStatus next)
    {
        switch (Status)
        {
            case ScenarioScheduleStatus.Draft:
                switch (next)
                {
                    case ScenarioScheduleStatus.Pending:
                    case ScenarioScheduleStatus.Suspended:
                        return true;
                }
                return false;
            case ScenarioScheduleStatus.Pending:
                return next == ScenarioScheduleStatus.Active;
            case ScenarioScheduleStatus.Active:
                return next == ScenarioScheduleStatus.Closed;
            case ScenarioScheduleStatus.Suspended:
                switch (next)
                {
                    case ScenarioScheduleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)BatchSize + Rating, 2);
    }

    public ScenarioSchedule Copy()
    {
        return new ScenarioSchedule(
            Code,
            Name,
            Version,
            BatchSize,
            ExternalRef,
            Unit,
            Discount,
            Summary,
            Priority,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
