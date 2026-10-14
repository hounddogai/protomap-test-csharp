namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark entry is in its life cycle.</summary>
public enum BenchmarkEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One benchmark entry, with what the planners need to act on it.</summary>
public sealed class BenchmarkEntry(string code, string title, bool visible, int revision, int batchSize, int sequence, string externalRef, int attempts, BenchmarkEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public int Revision { get; set; } = revision;
    public int BatchSize { get; set; } = batchSize;
    public int Sequence { get; set; } = sequence;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public BenchmarkEntryStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark entry in status planned.</summary>
    public static BenchmarkEntry NewBenchmarkEntry(string code, string title)
    {
        return new BenchmarkEntry(code, title, false, 0, 0, 0, "", 0, BenchmarkEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkEntryStatus next)
    {
        switch (Status)
        {
            case BenchmarkEntryStatus.Planned:
                switch (next)
                {
                    case BenchmarkEntryStatus.Scheduled:
                        return true;
                }
                return false;
            case BenchmarkEntryStatus.Scheduled:
                return next == BenchmarkEntryStatus.InProgress;
            case BenchmarkEntryStatus.InProgress:
                switch (next)
                {
                    case BenchmarkEntryStatus.Completed:
                    case BenchmarkEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case BenchmarkEntryStatus.Completed:
                switch (next)
                {
                    case BenchmarkEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts - (double)Sequence, 2);
    }

    public BenchmarkEntry Copy()
    {
        return new BenchmarkEntry(
            Code,
            Title,
            Visible,
            Revision,
            BatchSize,
            Sequence,
            ExternalRef,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
