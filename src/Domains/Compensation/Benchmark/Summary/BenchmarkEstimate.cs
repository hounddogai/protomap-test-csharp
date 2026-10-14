namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark estimate is in its life cycle.</summary>
public enum BenchmarkEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A benchmark estimate as the benchmark team keeps it.</summary>
public sealed class BenchmarkEstimate(string code, string title, string phone, double rate, double margin, int capacity, bool pinned, string currency, int version, double cost, BenchmarkEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public int Capacity { get; set; } = capacity;
    public bool Pinned { get; set; } = pinned;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public double Cost { get; set; } = cost;
    public BenchmarkEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark estimate in status idle.</summary>
    public static BenchmarkEstimate NewBenchmarkEstimate(string code, string title)
    {
        return new BenchmarkEstimate(code, title, "", 0.0, 0.0, 0, false, "", 0, 0.0, BenchmarkEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkEstimateStatus next)
    {
        switch (Status)
        {
            case BenchmarkEstimateStatus.Idle:
                return next == BenchmarkEstimateStatus.Warming || next == BenchmarkEstimateStatus.Stopped;
            case BenchmarkEstimateStatus.Warming:
                return next == BenchmarkEstimateStatus.Ready;
            case BenchmarkEstimateStatus.Ready:
                return next == BenchmarkEstimateStatus.Stopped;
            case BenchmarkEstimateStatus.Draining:
                return next == BenchmarkEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BenchmarkEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Capacity * Cost, 2);
    }

    public BenchmarkEstimate Copy()
    {
        return new BenchmarkEstimate(
            Code,
            Title,
            Phone,
            Rate,
            Margin,
            Capacity,
            Pinned,
            Currency,
            Version,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
