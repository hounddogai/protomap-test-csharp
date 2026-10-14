namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped span rule is in its life cycle.</summary>
public enum CappedSpanRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped span rule, with what the planners need to act on it.</summary>
public sealed class CappedSpanRule(string code, string label, double fte, int batchSize, string unit, CappedSpanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public CappedSpanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped span rule in status idle.</summary>
    public static CappedSpanRule NewCappedSpanRule(string code, string label)
    {
        return new CappedSpanRule(code, label, 0.0, 0, "", CappedSpanRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSpanRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSpanRuleStatus next)
    {
        switch (Status)
        {
            case CappedSpanRuleStatus.Idle:
                return next == CappedSpanRuleStatus.Warming;
            case CappedSpanRuleStatus.Warming:
                switch (next)
                {
                    case CappedSpanRuleStatus.Ready:
                    case CappedSpanRuleStatus.Draining:
                        return true;
                }
                return false;
            case CappedSpanRuleStatus.Ready:
                return next == CappedSpanRuleStatus.Draining;
            case CappedSpanRuleStatus.Draining:
                return next == CappedSpanRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedSpanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped span rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Fte + (double)BatchSize, 2);
    }

    public CappedSpanRule Copy()
    {
        return new CappedSpanRule(Code, Label, Fte, BatchSize, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
