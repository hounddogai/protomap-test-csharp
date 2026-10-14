namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark claim is in its life cycle.</summary>
public enum BenchmarkClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark claim. Its code is unique within a tenant.</summary>
public sealed class BenchmarkClaim(string code, string name, int priority, string costCenter, bool archived, double amount, string timezone, int batchSize, double cost, string locale, BenchmarkClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public int BatchSize { get; set; } = batchSize;
    public double Cost { get; set; } = cost;
    public string Locale { get; set; } = locale;
    public BenchmarkClaimStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark claim in status new.</summary>
    public static BenchmarkClaim NewBenchmarkClaim(string code, string name)
    {
        return new BenchmarkClaim(code, name, 0, "", false, 0.0, "", 0, 0.0, "", BenchmarkClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkClaimStatus next)
    {
        switch (Status)
        {
            case BenchmarkClaimStatus.New:
                switch (next)
                {
                    case BenchmarkClaimStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkClaimStatus.Queued:
                return next == BenchmarkClaimStatus.Running;
            case BenchmarkClaimStatus.Running:
                return next == BenchmarkClaimStatus.Failed;
            case BenchmarkClaimStatus.Done:
                switch (next)
                {
                    case BenchmarkClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark claim"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority * Amount, 2);
    }

    public BenchmarkClaim Copy()
    {
        return new BenchmarkClaim(
            Code,
            Name,
            Priority,
            CostCenter,
            Archived,
            Amount,
            Timezone,
            BatchSize,
            Cost,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
