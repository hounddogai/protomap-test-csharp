namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched benchmark entry is in its life cycle.</summary>
public enum BatchedBenchmarkEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a batched benchmark entry and its current status.</summary>
public sealed class BatchedBenchmarkEntry(string code, string title, string employeeId, double fte, double ratio, int sequence, string costCenter, int durationMinutes, double score, string reviewer, int startsAt, BatchedBenchmarkEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Fte { get; set; } = fte;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public string CostCenter { get; set; } = costCenter;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public int StartsAt { get; set; } = startsAt;
    public BatchedBenchmarkEntryStatus Status { get; set; } = status;

    /// <summary>Starts a batched benchmark entry in status open.</summary>
    public static BatchedBenchmarkEntry NewBatchedBenchmarkEntry(string code, string title)
    {
        return new BatchedBenchmarkEntry(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            "",
            0,
            0.0,
            "",
            0,
            BatchedBenchmarkEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBenchmarkEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBenchmarkEntryStatus next)
    {
        switch (Status)
        {
            case BatchedBenchmarkEntryStatus.Open:
                return next == BatchedBenchmarkEntryStatus.InReview || next == BatchedBenchmarkEntryStatus.Archived;
            case BatchedBenchmarkEntryStatus.InReview:
                switch (next)
                {
                    case BatchedBenchmarkEntryStatus.Resolved:
                        return true;
                }
                return false;
            case BatchedBenchmarkEntryStatus.Resolved:
                switch (next)
                {
                    case BatchedBenchmarkEntryStatus.Reopened:
                    case BatchedBenchmarkEntryStatus.Archived:
                        return true;
                }
                return false;
            case BatchedBenchmarkEntryStatus.Reopened:
                return next == BatchedBenchmarkEntryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedBenchmarkEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched benchmark entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched benchmark entry"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score - (double)DurationMinutes, 2);
    }

    public BatchedBenchmarkEntry Copy()
    {
        return new BatchedBenchmarkEntry(
            Code,
            Title,
            EmployeeId,
            Fte,
            Ratio,
            Sequence,
            CostCenter,
            DurationMinutes,
            Score,
            Reviewer,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
