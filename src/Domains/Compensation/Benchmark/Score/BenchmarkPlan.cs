namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark plan is in its life cycle.</summary>
public enum BenchmarkPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A benchmark plan as the benchmark team keeps it.</summary>
public sealed class BenchmarkPlan(string code, string label, string source, string jobCode, string reviewer, double minimum, double threshold, int capacity, string name, BenchmarkPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string JobCode { get; set; } = jobCode;
    public string Reviewer { get; set; } = reviewer;
    public double Minimum { get; set; } = minimum;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public BenchmarkPlanStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark plan in status idle.</summary>
    public static BenchmarkPlan NewBenchmarkPlan(string code, string label)
    {
        return new BenchmarkPlan(code, label, "", "", "", 0.0, 0.0, 0, "", BenchmarkPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkPlanStatus next)
    {
        switch (Status)
        {
            case BenchmarkPlanStatus.Idle:
                return next == BenchmarkPlanStatus.Warming;
            case BenchmarkPlanStatus.Warming:
                return next == BenchmarkPlanStatus.Ready || next == BenchmarkPlanStatus.Draining;
            case BenchmarkPlanStatus.Ready:
                switch (next)
                {
                    case BenchmarkPlanStatus.Draining:
                    case BenchmarkPlanStatus.Stopped:
                        return true;
                }
                return false;
            case BenchmarkPlanStatus.Draining:
                switch (next)
                {
                    case BenchmarkPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"benchmark plan"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold + Minimum, 2);
    }

    public BenchmarkPlan Copy()
    {
        return new BenchmarkPlan(Code, Label, Source, JobCode, Reviewer, Minimum, Threshold, Capacity, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
