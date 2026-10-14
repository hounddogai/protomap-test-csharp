namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario group is in its life cycle.</summary>
public enum ScenarioGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scenario group, with what the planners need to act on it.</summary>
public sealed class ScenarioGroup(string code, string title, double budget, double margin, string phone, string label, ScenarioGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public string Label { get; set; } = label;
    public ScenarioGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scenario group in status new.</summary>
    public static ScenarioGroup NewScenarioGroup(string code, string title)
    {
        return new ScenarioGroup(code, title, 0.0, 0.0, "", "", ScenarioGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioGroupStatus next)
    {
        switch (Status)
        {
            case ScenarioGroupStatus.New:
                switch (next)
                {
                    case ScenarioGroupStatus.Done:
                    case ScenarioGroupStatus.Failed:
                        return true;
                }
                return false;
            case ScenarioGroupStatus.Queued:
                return next == ScenarioGroupStatus.Failed;
            case ScenarioGroupStatus.Running:
                return next == ScenarioGroupStatus.Failed;
            case ScenarioGroupStatus.Done:
                return next == ScenarioGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScenarioGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scenario group"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Budget * Margin, 2);
    }

    public ScenarioGroup Copy()
    {
        return new ScenarioGroup(Code, Title, Budget, Margin, Phone, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
