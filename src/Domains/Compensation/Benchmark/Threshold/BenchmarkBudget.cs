namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark budget is in its life cycle.</summary>
public enum BenchmarkBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A benchmark budget. Its code is unique within a tenant.</summary>
public sealed class BenchmarkBudget(string code, string title, int limit, string badge, int quantity, int batchSize, string employeeId, string description, string segment, BenchmarkBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public int BatchSize { get; set; } = batchSize;
    public string EmployeeId { get; set; } = employeeId;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public BenchmarkBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark budget in status planned.</summary>
    public static BenchmarkBudget NewBenchmarkBudget(string code, string title)
    {
        return new BenchmarkBudget(code, title, 0, "", 0, 0, "", "", "", BenchmarkBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkBudgetStatus next)
    {
        switch (Status)
        {
            case BenchmarkBudgetStatus.Planned:
                return next == BenchmarkBudgetStatus.InProgress;
            case BenchmarkBudgetStatus.Scheduled:
                switch (next)
                {
                    case BenchmarkBudgetStatus.Completed:
                    case BenchmarkBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case BenchmarkBudgetStatus.InProgress:
                return next == BenchmarkBudgetStatus.Completed;
            case BenchmarkBudgetStatus.Completed:
                switch (next)
                {
                    case BenchmarkBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark budget"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Quantity + (double)Limit, 2);
    }

    public BenchmarkBudget Copy()
    {
        return new BenchmarkBudget(
            Code,
            Title,
            Limit,
            Badge,
            Quantity,
            BatchSize,
            EmployeeId,
            Description,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
