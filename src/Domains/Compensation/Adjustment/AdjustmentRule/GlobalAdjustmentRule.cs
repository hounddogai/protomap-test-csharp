namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global adjustment rule is in its life cycle.</summary>
public enum GlobalAdjustmentRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One global adjustment rule, with what the planners need to act on it.</summary>
public sealed class GlobalAdjustmentRule(string code, string label, int sequence, int endsAt, bool visible, int limit, int attempts, double weight, GlobalAdjustmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public GlobalAdjustmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a global adjustment rule in status open.</summary>
    public static GlobalAdjustmentRule NewGlobalAdjustmentRule(string code, string label)
    {
        return new GlobalAdjustmentRule(code, label, 0, 0, false, 0, 0, 0.0, GlobalAdjustmentRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAdjustmentRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAdjustmentRuleStatus next)
    {
        switch (Status)
        {
            case GlobalAdjustmentRuleStatus.Open:
                switch (next)
                {
                    case GlobalAdjustmentRuleStatus.InReview:
                        return true;
                }
                return false;
            case GlobalAdjustmentRuleStatus.InReview:
                return next == GlobalAdjustmentRuleStatus.Reopened;
            case GlobalAdjustmentRuleStatus.Resolved:
                switch (next)
                {
                    case GlobalAdjustmentRuleStatus.Reopened:
                    case GlobalAdjustmentRuleStatus.Archived:
                        return true;
                }
                return false;
            case GlobalAdjustmentRuleStatus.Reopened:
                return next == GlobalAdjustmentRuleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalAdjustmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global adjustment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global adjustment rule"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)EndsAt - (double)Limit, 2);
    }

    public GlobalAdjustmentRule Copy()
    {
        return new GlobalAdjustmentRule(Code, Label, Sequence, EndsAt, Visible, Limit, Attempts, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
