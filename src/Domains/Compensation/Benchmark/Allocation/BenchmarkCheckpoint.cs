namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark checkpoint is in its life cycle.</summary>
public enum BenchmarkCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A benchmark checkpoint. Its code is unique within a tenant.</summary>
public sealed class BenchmarkCheckpoint(string code, string name, bool pinned, double hours, int batchSize, int createdAt, int updatedAt, BenchmarkCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public BenchmarkCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark checkpoint in status open.</summary>
    public static BenchmarkCheckpoint NewBenchmarkCheckpoint(string code, string name)
    {
        return new BenchmarkCheckpoint(code, name, false, 0.0, 0, 0, 0, BenchmarkCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkCheckpointStatus next)
    {
        switch (Status)
        {
            case BenchmarkCheckpointStatus.Open:
                switch (next)
                {
                    case BenchmarkCheckpointStatus.Resolved:
                        return true;
                }
                return false;
            case BenchmarkCheckpointStatus.InReview:
                return next == BenchmarkCheckpointStatus.Resolved || next == BenchmarkCheckpointStatus.Reopened;
            case BenchmarkCheckpointStatus.Resolved:
                switch (next)
                {
                    case BenchmarkCheckpointStatus.Reopened:
                    case BenchmarkCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case BenchmarkCheckpointStatus.Reopened:
                return next == BenchmarkCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BenchmarkCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"benchmark checkpoint"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)BatchSize + Hours, 2);
    }

    public BenchmarkCheckpoint Copy()
    {
        return new BenchmarkCheckpoint(Code, Name, Pinned, Hours, BatchSize, CreatedAt, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
