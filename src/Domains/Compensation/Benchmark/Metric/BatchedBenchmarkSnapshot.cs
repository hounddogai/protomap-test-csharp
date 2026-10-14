namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched benchmark snapshot is in its life cycle.</summary>
public enum BatchedBenchmarkSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a batched benchmark snapshot and its current status.</summary>
public sealed class BatchedBenchmarkSnapshot(string code, string name, string segment, double ceiling, double score, bool archived, int createdAt, int sequence, int leaveDays, BatchedBenchmarkSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public int CreatedAt { get; set; } = createdAt;
    public int Sequence { get; set; } = sequence;
    public int LeaveDays { get; set; } = leaveDays;
    public BatchedBenchmarkSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched benchmark snapshot in status created.</summary>
    public static BatchedBenchmarkSnapshot NewBatchedBenchmarkSnapshot(string code, string name)
    {
        return new BatchedBenchmarkSnapshot(
            code,
            name,
            "",
            0.0,
            0.0,
            false,
            0,
            0,
            0,
            BatchedBenchmarkSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBenchmarkSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBenchmarkSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedBenchmarkSnapshotStatus.Created:
                return next == BatchedBenchmarkSnapshotStatus.Validated || next == BatchedBenchmarkSnapshotStatus.Retired;
            case BatchedBenchmarkSnapshotStatus.Validated:
                return next == BatchedBenchmarkSnapshotStatus.Published || next == BatchedBenchmarkSnapshotStatus.Retired;
            case BatchedBenchmarkSnapshotStatus.Published:
                return next == BatchedBenchmarkSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedBenchmarkSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched benchmark snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched benchmark snapshot"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Score - Ceiling, 2);
    }

    public BatchedBenchmarkSnapshot Copy()
    {
        return new BatchedBenchmarkSnapshot(
            Code,
            Name,
            Segment,
            Ceiling,
            Score,
            Archived,
            CreatedAt,
            Sequence,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
