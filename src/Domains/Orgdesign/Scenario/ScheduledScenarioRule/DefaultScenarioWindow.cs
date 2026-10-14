namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default scenario window is in its life cycle.</summary>
public enum DefaultScenarioWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default scenario window, with what the planners need to act on it.</summary>
public sealed class DefaultScenarioWindow(string code, string label, string reviewer, bool pinned, double score, DefaultScenarioWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public double Score { get; set; } = score;
    public DefaultScenarioWindowStatus Status { get; set; } = status;

    /// <summary>Starts a default scenario window in status new.</summary>
    public static DefaultScenarioWindow NewDefaultScenarioWindow(string code, string label)
    {
        return new DefaultScenarioWindow(code, label, "", false, 0.0, DefaultScenarioWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultScenarioWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultScenarioWindowStatus next)
    {
        switch (Status)
        {
            case DefaultScenarioWindowStatus.New:
                return next == DefaultScenarioWindowStatus.Running;
            case DefaultScenarioWindowStatus.Queued:
                return next == DefaultScenarioWindowStatus.Running || next == DefaultScenarioWindowStatus.Failed;
            case DefaultScenarioWindowStatus.Running:
                return next == DefaultScenarioWindowStatus.Done;
            case DefaultScenarioWindowStatus.Done:
                return next == DefaultScenarioWindowStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultScenarioWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default scenario window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default scenario window"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public DefaultScenarioWindow Copy()
    {
        return new DefaultScenarioWindow(Code, Label, Reviewer, Pinned, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
