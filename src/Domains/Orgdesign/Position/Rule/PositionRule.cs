namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position rule is in its life cycle.</summary>
public enum PositionRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One position rule, with what the planners need to act on it.</summary>
public sealed class PositionRule(string code, string label, string managerId, string skill, string contactEmail, double amount, PositionRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string Skill { get; set; } = skill;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public PositionRuleStatus Status { get; set; } = status;

    /// <summary>Starts a position rule in status idle.</summary>
    public static PositionRule NewPositionRule(string code, string label)
    {
        return new PositionRule(code, label, "", "", "", 0.0, PositionRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionRuleStatus next)
    {
        switch (Status)
        {
            case PositionRuleStatus.Idle:
                return next == PositionRuleStatus.Warming || next == PositionRuleStatus.Draining;
            case PositionRuleStatus.Warming:
                switch (next)
                {
                    case PositionRuleStatus.Stopped:
                        return true;
                }
                return false;
            case PositionRuleStatus.Ready:
                switch (next)
                {
                    case PositionRuleStatus.Draining:
                        return true;
                }
                return false;
            case PositionRuleStatus.Draining:
                return next == PositionRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PositionRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PositionRule Copy()
    {
        return new PositionRule(Code, Label, ManagerId, Skill, ContactEmail, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
