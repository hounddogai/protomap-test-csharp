namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark batch is in its life cycle.</summary>
public enum BenchmarkBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A benchmark batch as the benchmark team keeps it.</summary>
public sealed class BenchmarkBatch(string code, string label, int createdAt, int windowDays, int expiresAt, BenchmarkBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public BenchmarkBatchStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark batch in status open.</summary>
    public static BenchmarkBatch NewBenchmarkBatch(string code, string label)
    {
        return new BenchmarkBatch(code, label, 0, 0, 0, BenchmarkBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkBatchStatus next)
    {
        switch (Status)
        {
            case BenchmarkBatchStatus.Open:
                return next == BenchmarkBatchStatus.InReview || next == BenchmarkBatchStatus.Resolved;
            case BenchmarkBatchStatus.InReview:
                switch (next)
                {
                    case BenchmarkBatchStatus.Resolved:
                        return true;
                }
                return false;
            case BenchmarkBatchStatus.Resolved:
                return next == BenchmarkBatchStatus.Archived;
            case BenchmarkBatchStatus.Reopened:
                return next == BenchmarkBatchStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BenchmarkBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt * (double)CreatedAt, 2);
    }

    public BenchmarkBatch Copy()
    {
        return new BenchmarkBatch(Code, Label, CreatedAt, WindowDays, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
