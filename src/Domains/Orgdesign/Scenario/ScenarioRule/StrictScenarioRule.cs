namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict scenario rule is in its life cycle.</summary>
public enum StrictScenarioRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict scenario rule, with what the planners need to act on it.</summary>
public sealed class StrictScenarioRule(string code, string label, string reviewer, string notes, double threshold, string category, string salaryBand, int updatedAt, string location, StrictScenarioRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public double Threshold { get; set; } = threshold;
    public string Category { get; set; } = category;
    public string SalaryBand { get; set; } = salaryBand;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public StrictScenarioRuleStatus Status { get; set; } = status;

    /// <summary>Starts a strict scenario rule in status proposed.</summary>
    public static StrictScenarioRule NewStrictScenarioRule(string code, string label)
    {
        return new StrictScenarioRule(code, label, "", "", 0.0, "", "", 0, "", StrictScenarioRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictScenarioRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictScenarioRuleStatus next)
    {
        switch (Status)
        {
            case StrictScenarioRuleStatus.Proposed:
                return next == StrictScenarioRuleStatus.Approved || next == StrictScenarioRuleStatus.Withdrawn;
            case StrictScenarioRuleStatus.Approved:
                switch (next)
                {
                    case StrictScenarioRuleStatus.Rejected:
                        return true;
                }
                return false;
            case StrictScenarioRuleStatus.Rejected:
                switch (next)
                {
                    case StrictScenarioRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictScenarioRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict scenario rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold + (double)UpdatedAt, 2);
    }

    public StrictScenarioRule Copy()
    {
        return new StrictScenarioRule(
            Code,
            Label,
            Reviewer,
            Notes,
            Threshold,
            Category,
            SalaryBand,
            UpdatedAt,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
