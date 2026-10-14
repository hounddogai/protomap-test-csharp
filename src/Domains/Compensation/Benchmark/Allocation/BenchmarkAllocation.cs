namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark allocation is in its life cycle.</summary>
public enum BenchmarkAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a benchmark allocation and its current status.</summary>
public sealed class BenchmarkAllocation(string code, string label, int endsAt, int revision, string country, int limit, string segment, BenchmarkAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public string Country { get; set; } = country;
    public int Limit { get; set; } = limit;
    public string Segment { get; set; } = segment;
    public BenchmarkAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark allocation in status open.</summary>
    public static BenchmarkAllocation NewBenchmarkAllocation(string code, string label)
    {
        return new BenchmarkAllocation(code, label, 0, 0, "", 0, "", BenchmarkAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkAllocationStatus next)
    {
        switch (Status)
        {
            case BenchmarkAllocationStatus.Open:
                switch (next)
                {
                    case BenchmarkAllocationStatus.Archived:
                        return true;
                }
                return false;
            case BenchmarkAllocationStatus.InReview:
                return next == BenchmarkAllocationStatus.Resolved;
            case BenchmarkAllocationStatus.Resolved:
                return next == BenchmarkAllocationStatus.Reopened || next == BenchmarkAllocationStatus.Archived;
            case BenchmarkAllocationStatus.Reopened:
                switch (next)
                {
                    case BenchmarkAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"benchmark allocation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit * (double)Revision, 2);
    }

    public BenchmarkAllocation Copy()
    {
        return new BenchmarkAllocation(Code, Label, EndsAt, Revision, Country, Limit, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
