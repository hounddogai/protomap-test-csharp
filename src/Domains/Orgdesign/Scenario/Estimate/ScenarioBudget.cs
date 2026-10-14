namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario budget is in its life cycle.</summary>
public enum ScenarioBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scenario budget and its current status.</summary>
public sealed class ScenarioBudget(string code, string label, double hours, bool approved, double margin, string badge, double weight, string segment, int updatedAt, string skill, double ratio, ScenarioBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public bool Approved { get; set; } = approved;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public double Ratio { get; set; } = ratio;
    public ScenarioBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a scenario budget in status proposed.</summary>
    public static ScenarioBudget NewScenarioBudget(string code, string label)
    {
        return new ScenarioBudget(
            code,
            label,
            0.0,
            false,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            0.0,
            ScenarioBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioBudgetStatus next)
    {
        switch (Status)
        {
            case ScenarioBudgetStatus.Proposed:
                return next == ScenarioBudgetStatus.Rejected;
            case ScenarioBudgetStatus.Approved:
                switch (next)
                {
                    case ScenarioBudgetStatus.Rejected:
                    case ScenarioBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScenarioBudgetStatus.Rejected:
                switch (next)
                {
                    case ScenarioBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours - (double)UpdatedAt, 2);
    }

    public ScenarioBudget Copy()
    {
        return new ScenarioBudget(
            Code,
            Label,
            Hours,
            Approved,
            Margin,
            Badge,
            Weight,
            Segment,
            UpdatedAt,
            Skill,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
