namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario window is in its life cycle.</summary>
public enum ScenarioWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scenario window as the scenario team keeps it.</summary>
public sealed class ScenarioWindow(string code, string label, string notes, string currency, double minimum, double ratio, int durationMinutes, int capacity, double weight, double ceiling, string badge, ScenarioWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Currency { get; set; } = currency;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public double Weight { get; set; } = weight;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public ScenarioWindowStatus Status { get; set; } = status;

    /// <summary>Starts a scenario window in status open.</summary>
    public static ScenarioWindow NewScenarioWindow(string code, string label)
    {
        return new ScenarioWindow(code, label, "", "", 0.0, 0.0, 0, 0, 0.0, 0.0, "", ScenarioWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioWindowStatus next)
    {
        switch (Status)
        {
            case ScenarioWindowStatus.Open:
                return next == ScenarioWindowStatus.Resolved || next == ScenarioWindowStatus.Archived;
            case ScenarioWindowStatus.InReview:
                return next == ScenarioWindowStatus.Reopened;
            case ScenarioWindowStatus.Resolved:
                return next == ScenarioWindowStatus.Reopened || next == ScenarioWindowStatus.Archived;
            case ScenarioWindowStatus.Reopened:
                return next == ScenarioWindowStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScenarioWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario window"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ratio - (double)Capacity, 2);
    }

    public ScenarioWindow Copy()
    {
        return new ScenarioWindow(
            Code,
            Label,
            Notes,
            Currency,
            Minimum,
            Ratio,
            DurationMinutes,
            Capacity,
            Weight,
            Ceiling,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
