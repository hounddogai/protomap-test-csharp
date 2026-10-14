namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback benchmark rule is in its life cycle.</summary>
public enum FallbackBenchmarkRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback benchmark rule as the benchmark team keeps it.</summary>
public sealed class FallbackBenchmarkRule(string code, string label, bool visible, string team, string segment, double amount, FallbackBenchmarkRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public string Segment { get; set; } = segment;
    public double Amount { get; set; } = amount;
    public FallbackBenchmarkRuleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback benchmark rule in status proposed.</summary>
    public static FallbackBenchmarkRule NewFallbackBenchmarkRule(string code, string label)
    {
        return new FallbackBenchmarkRule(code, label, false, "", "", 0.0, FallbackBenchmarkRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBenchmarkRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBenchmarkRuleStatus next)
    {
        switch (Status)
        {
            case FallbackBenchmarkRuleStatus.Proposed:
                return next == FallbackBenchmarkRuleStatus.Approved || next == FallbackBenchmarkRuleStatus.Rejected;
            case FallbackBenchmarkRuleStatus.Approved:
                return next == FallbackBenchmarkRuleStatus.Rejected || next == FallbackBenchmarkRuleStatus.Withdrawn;
            case FallbackBenchmarkRuleStatus.Rejected:
                return next == FallbackBenchmarkRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackBenchmarkRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback benchmark rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback benchmark rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public FallbackBenchmarkRule Copy()
    {
        return new FallbackBenchmarkRule(Code, Label, Visible, Team, Segment, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
