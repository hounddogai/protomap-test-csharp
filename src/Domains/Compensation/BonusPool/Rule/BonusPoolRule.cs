namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool rule is in its life cycle.</summary>
public enum BonusPoolRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a bonus pool rule and its current status.</summary>
public sealed class BonusPoolRule(string code, string label, bool approved, string channel, double rate, BonusPoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Channel { get; set; } = channel;
    public double Rate { get; set; } = rate;
    public BonusPoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool rule in status idle.</summary>
    public static BonusPoolRule NewBonusPoolRule(string code, string label)
    {
        return new BonusPoolRule(code, label, false, "", 0.0, BonusPoolRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolRuleStatus next)
    {
        switch (Status)
        {
            case BonusPoolRuleStatus.Idle:
                return next == BonusPoolRuleStatus.Ready;
            case BonusPoolRuleStatus.Warming:
                return next == BonusPoolRuleStatus.Ready || next == BonusPoolRuleStatus.Stopped;
            case BonusPoolRuleStatus.Ready:
                return next == BonusPoolRuleStatus.Draining || next == BonusPoolRuleStatus.Stopped;
            case BonusPoolRuleStatus.Draining:
                return next == BonusPoolRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BonusPoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public BonusPoolRule Copy()
    {
        return new BonusPoolRule(Code, Label, Approved, Channel, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
