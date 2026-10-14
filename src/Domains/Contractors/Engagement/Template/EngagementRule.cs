namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement rule is in its life cycle.</summary>
public enum EngagementRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A engagement rule. Its code is unique within a tenant.</summary>
public sealed class EngagementRule(string code, string name, int sequence, double amount, bool archived, double weight, string title, int retries, EngagementRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public EngagementRuleStatus Status { get; set; } = status;

    /// <summary>Starts a engagement rule in status idle.</summary>
    public static EngagementRule NewEngagementRule(string code, string name)
    {
        return new EngagementRule(code, name, 0, 0.0, false, 0.0, "", 0, EngagementRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementRuleStatus next)
    {
        switch (Status)
        {
            case EngagementRuleStatus.Idle:
                switch (next)
                {
                    case EngagementRuleStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementRuleStatus.Warming:
                switch (next)
                {
                    case EngagementRuleStatus.Draining:
                        return true;
                }
                return false;
            case EngagementRuleStatus.Ready:
                switch (next)
                {
                    case EngagementRuleStatus.Draining:
                        return true;
                }
                return false;
            case EngagementRuleStatus.Draining:
                return next == EngagementRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EngagementRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight - Amount, 2);
    }

    public EngagementRule Copy()
    {
        return new EngagementRule(Code, Name, Sequence, Amount, Archived, Weight, Title, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
