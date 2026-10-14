namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario rule is in its life cycle.</summary>
public enum ScenarioRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scenario rule and its current status.</summary>
public sealed class ScenarioRule(string code, string label, string reviewer, string category, int endsAt, string costCenter, string timezone, ScenarioRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public ScenarioRuleStatus Status { get; set; } = status;

    /// <summary>Starts a scenario rule in status draft.</summary>
    public static ScenarioRule NewScenarioRule(string code, string label)
    {
        return new ScenarioRule(code, label, "", "", 0, "", "", ScenarioRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioRuleStatus next)
    {
        switch (Status)
        {
            case ScenarioRuleStatus.Draft:
                switch (next)
                {
                    case ScenarioRuleStatus.Pending:
                    case ScenarioRuleStatus.Active:
                        return true;
                }
                return false;
            case ScenarioRuleStatus.Pending:
                return next == ScenarioRuleStatus.Suspended || next == ScenarioRuleStatus.Closed;
            case ScenarioRuleStatus.Active:
                switch (next)
                {
                    case ScenarioRuleStatus.Suspended:
                    case ScenarioRuleStatus.Closed:
                        return true;
                }
                return false;
            case ScenarioRuleStatus.Suspended:
                return next == ScenarioRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScenarioRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public ScenarioRule Copy()
    {
        return new ScenarioRule(Code, Label, Reviewer, Category, EndsAt, CostCenter, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
