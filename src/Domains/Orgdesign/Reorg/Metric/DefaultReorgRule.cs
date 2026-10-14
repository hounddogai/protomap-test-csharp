namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default reorg rule is in its life cycle.</summary>
public enum DefaultReorgRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default reorg rule. Its code is unique within a tenant.</summary>
public sealed class DefaultReorgRule(string code, string name, string department, double minimum, int attempts, DefaultReorgRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Minimum { get; set; } = minimum;
    public int Attempts { get; set; } = attempts;
    public DefaultReorgRuleStatus Status { get; set; } = status;

    /// <summary>Starts a default reorg rule in status idle.</summary>
    public static DefaultReorgRule NewDefaultReorgRule(string code, string name)
    {
        return new DefaultReorgRule(code, name, "", 0.0, 0, DefaultReorgRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReorgRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReorgRuleStatus next)
    {
        switch (Status)
        {
            case DefaultReorgRuleStatus.Idle:
                return next == DefaultReorgRuleStatus.Draining || next == DefaultReorgRuleStatus.Stopped;
            case DefaultReorgRuleStatus.Warming:
                return next == DefaultReorgRuleStatus.Draining || next == DefaultReorgRuleStatus.Stopped;
            case DefaultReorgRuleStatus.Ready:
                return next == DefaultReorgRuleStatus.Draining || next == DefaultReorgRuleStatus.Stopped;
            case DefaultReorgRuleStatus.Draining:
                switch (next)
                {
                    case DefaultReorgRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultReorgRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default reorg rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default reorg rule"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - Minimum, 2);
    }

    public DefaultReorgRule Copy()
    {
        return new DefaultReorgRule(Code, Name, Department, Minimum, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
