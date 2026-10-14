namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool rule is in its life cycle.</summary>
public enum PoolRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pool rule. Its code is unique within a tenant.</summary>
public sealed class PoolRule(string code, string label, double margin, bool active, string title, PoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public string Title { get; set; } = title;
    public PoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pool rule in status new.</summary>
    public static PoolRule NewPoolRule(string code, string label)
    {
        return new PoolRule(code, label, 0.0, false, "", PoolRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolRuleStatus next)
    {
        switch (Status)
        {
            case PoolRuleStatus.New:
                return next == PoolRuleStatus.Running;
            case PoolRuleStatus.Queued:
                switch (next)
                {
                    case PoolRuleStatus.Done:
                        return true;
                }
                return false;
            case PoolRuleStatus.Running:
                switch (next)
                {
                    case PoolRuleStatus.Done:
                    case PoolRuleStatus.Failed:
                        return true;
                }
                return false;
            case PoolRuleStatus.Done:
                switch (next)
                {
                    case PoolRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pool rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public PoolRule Copy()
    {
        return new PoolRule(Code, Label, Margin, Active, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
