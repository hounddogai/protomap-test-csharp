namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual scenario rule is in its life cycle.</summary>
public enum ManualScenarioRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual scenario rule and its current status.</summary>
public sealed class ManualScenarioRule(string code, string label, string timezone, int attempts, int priority, string location, ManualScenarioRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public string Location { get; set; } = location;
    public ManualScenarioRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual scenario rule in status new.</summary>
    public static ManualScenarioRule NewManualScenarioRule(string code, string label)
    {
        return new ManualScenarioRule(code, label, "", 0, 0, "", ManualScenarioRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualScenarioRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualScenarioRuleStatus next)
    {
        switch (Status)
        {
            case ManualScenarioRuleStatus.New:
                return next == ManualScenarioRuleStatus.Running;
            case ManualScenarioRuleStatus.Queued:
                return next == ManualScenarioRuleStatus.Running || next == ManualScenarioRuleStatus.Failed;
            case ManualScenarioRuleStatus.Running:
                switch (next)
                {
                    case ManualScenarioRuleStatus.Done:
                        return true;
                }
                return false;
            case ManualScenarioRuleStatus.Done:
                switch (next)
                {
                    case ManualScenarioRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualScenarioRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual scenario rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts - (double)Priority, 2);
    }

    public ManualScenarioRule Copy()
    {
        return new ManualScenarioRule(Code, Label, Timezone, Attempts, Priority, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
