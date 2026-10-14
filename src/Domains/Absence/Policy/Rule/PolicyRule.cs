namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy rule is in its life cycle.</summary>
public enum PolicyRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A policy rule. Its code is unique within a tenant.</summary>
public sealed class PolicyRule(string code, string name, string skill, string currency, Dictionary<string, string> attributes, int windowDays, PolicyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int WindowDays { get; set; } = windowDays;
    public PolicyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a policy rule in status new.</summary>
    public static PolicyRule NewPolicyRule(string code, string name)
    {
        return new PolicyRule(code, name, "", "", new Dictionary<string, string>(), 0, PolicyRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyRuleStatus next)
    {
        switch (Status)
        {
            case PolicyRuleStatus.New:
                return next == PolicyRuleStatus.Queued;
            case PolicyRuleStatus.Queued:
                return next == PolicyRuleStatus.Running;
            case PolicyRuleStatus.Running:
                switch (next)
                {
                    case PolicyRuleStatus.Done:
                    case PolicyRuleStatus.Failed:
                        return true;
                }
                return false;
            case PolicyRuleStatus.Done:
                return next == PolicyRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PolicyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public PolicyRule Copy()
    {
        return new PolicyRule(Code, Name, Skill, Currency, Attributes, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
