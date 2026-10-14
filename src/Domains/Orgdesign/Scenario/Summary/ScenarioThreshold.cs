namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario threshold is in its life cycle.</summary>
public enum ScenarioThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scenario threshold and its current status.</summary>
public sealed class ScenarioThreshold(string code, string label, string team, string grade, int startsAt, int windowDays, int limit, double fte, int revision, ScenarioThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public int Limit { get; set; } = limit;
    public double Fte { get; set; } = fte;
    public int Revision { get; set; } = revision;
    public ScenarioThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scenario threshold in status idle.</summary>
    public static ScenarioThreshold NewScenarioThreshold(string code, string label)
    {
        return new ScenarioThreshold(code, label, "", "", 0, 0, 0, 0.0, 0, ScenarioThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioThresholdStatus next)
    {
        switch (Status)
        {
            case ScenarioThresholdStatus.Idle:
                switch (next)
                {
                    case ScenarioThresholdStatus.Ready:
                    case ScenarioThresholdStatus.Draining:
                        return true;
                }
                return false;
            case ScenarioThresholdStatus.Warming:
                switch (next)
                {
                    case ScenarioThresholdStatus.Ready:
                        return true;
                }
                return false;
            case ScenarioThresholdStatus.Ready:
                switch (next)
                {
                    case ScenarioThresholdStatus.Draining:
                    case ScenarioThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioThresholdStatus.Draining:
                return next == ScenarioThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScenarioThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Fte - (double)StartsAt, 2);
    }

    public ScenarioThreshold Copy()
    {
        return new ScenarioThreshold(Code, Label, Team, Grade, StartsAt, WindowDays, Limit, Fte, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
