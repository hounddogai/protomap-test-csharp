namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark rule is in its life cycle.</summary>
public enum BenchmarkRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a benchmark rule and its current status.</summary>
public sealed class BenchmarkRule(string code, string label, int retries, double threshold, bool approved, BenchmarkRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public bool Approved { get; set; } = approved;
    public BenchmarkRuleStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark rule in status created.</summary>
    public static BenchmarkRule NewBenchmarkRule(string code, string label)
    {
        return new BenchmarkRule(code, label, 0, 0.0, false, BenchmarkRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkRuleStatus next)
    {
        switch (Status)
        {
            case BenchmarkRuleStatus.Created:
                switch (next)
                {
                    case BenchmarkRuleStatus.Validated:
                    case BenchmarkRuleStatus.Published:
                        return true;
                }
                return false;
            case BenchmarkRuleStatus.Validated:
                return next == BenchmarkRuleStatus.Published || next == BenchmarkRuleStatus.Retired;
            case BenchmarkRuleStatus.Published:
                return next == BenchmarkRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BenchmarkRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Retries + Threshold, 2);
    }

    public BenchmarkRule Copy()
    {
        return new BenchmarkRule(Code, Label, Retries, Threshold, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
