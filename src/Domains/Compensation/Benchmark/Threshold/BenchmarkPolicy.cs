namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark policy is in its life cycle.</summary>
public enum BenchmarkPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a benchmark policy and its current status.</summary>
public sealed class BenchmarkPolicy(string code, string name, double score, int retries, int batchSize, BenchmarkPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public BenchmarkPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark policy in status created.</summary>
    public static BenchmarkPolicy NewBenchmarkPolicy(string code, string name)
    {
        return new BenchmarkPolicy(code, name, 0.0, 0, 0, BenchmarkPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkPolicyStatus next)
    {
        switch (Status)
        {
            case BenchmarkPolicyStatus.Created:
                return next == BenchmarkPolicyStatus.Published;
            case BenchmarkPolicyStatus.Validated:
                switch (next)
                {
                    case BenchmarkPolicyStatus.Retired:
                        return true;
                }
                return false;
            case BenchmarkPolicyStatus.Published:
                switch (next)
                {
                    case BenchmarkPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark policy"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize * Score, 2);
    }

    public BenchmarkPolicy Copy()
    {
        return new BenchmarkPolicy(Code, Name, Score, Retries, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
