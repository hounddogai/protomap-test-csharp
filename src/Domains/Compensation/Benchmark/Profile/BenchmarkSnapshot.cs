namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark snapshot is in its life cycle.</summary>
public enum BenchmarkSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a benchmark snapshot and its current status.</summary>
public sealed class BenchmarkSnapshot(string code, string name, string category, string country, double cost, string skill, int quantity, int limit, BenchmarkSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public int Quantity { get; set; } = quantity;
    public int Limit { get; set; } = limit;
    public BenchmarkSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark snapshot in status open.</summary>
    public static BenchmarkSnapshot NewBenchmarkSnapshot(string code, string name)
    {
        return new BenchmarkSnapshot(code, name, "", "", 0.0, "", 0, 0, BenchmarkSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkSnapshotStatus next)
    {
        switch (Status)
        {
            case BenchmarkSnapshotStatus.Open:
                switch (next)
                {
                    case BenchmarkSnapshotStatus.Resolved:
                    case BenchmarkSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case BenchmarkSnapshotStatus.InReview:
                switch (next)
                {
                    case BenchmarkSnapshotStatus.Resolved:
                    case BenchmarkSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case BenchmarkSnapshotStatus.Resolved:
                return next == BenchmarkSnapshotStatus.Archived;
            case BenchmarkSnapshotStatus.Reopened:
                switch (next)
                {
                    case BenchmarkSnapshotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark snapshot"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Quantity * (double)Limit, 2);
    }

    public BenchmarkSnapshot Copy()
    {
        return new BenchmarkSnapshot(Code, Name, Category, Country, Cost, Skill, Quantity, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
