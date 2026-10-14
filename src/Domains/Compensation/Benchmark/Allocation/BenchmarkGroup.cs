namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark group is in its life cycle.</summary>
public enum BenchmarkGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A benchmark group. Its code is unique within a tenant.</summary>
public sealed class BenchmarkGroup(string code, string name, bool visible, double budget, double discount, bool approved, BenchmarkGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public BenchmarkGroupStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark group in status open.</summary>
    public static BenchmarkGroup NewBenchmarkGroup(string code, string name)
    {
        return new BenchmarkGroup(code, name, false, 0.0, 0.0, false, BenchmarkGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkGroupStatus next)
    {
        switch (Status)
        {
            case BenchmarkGroupStatus.Open:
                return next == BenchmarkGroupStatus.InReview;
            case BenchmarkGroupStatus.InReview:
                switch (next)
                {
                    case BenchmarkGroupStatus.Resolved:
                    case BenchmarkGroupStatus.Reopened:
                        return true;
                }
                return false;
            case BenchmarkGroupStatus.Resolved:
                return next == BenchmarkGroupStatus.Reopened;
            case BenchmarkGroupStatus.Reopened:
                return next == BenchmarkGroupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BenchmarkGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark group"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Discount + Budget, 2);
    }

    public BenchmarkGroup Copy()
    {
        return new BenchmarkGroup(Code, Name, Visible, Budget, Discount, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
