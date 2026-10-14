namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global benchmark override is in its life cycle.</summary>
public enum GlobalBenchmarkOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One global benchmark override, with what the planners need to act on it.</summary>
public sealed class GlobalBenchmarkOverride(string code, string label, int quantity, string costCenter, int createdAt, string category, int attempts, double weight, int graceDays, string segment, GlobalBenchmarkOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public string CostCenter { get; set; } = costCenter;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public int GraceDays { get; set; } = graceDays;
    public string Segment { get; set; } = segment;
    public GlobalBenchmarkOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a global benchmark override in status open.</summary>
    public static GlobalBenchmarkOverride NewGlobalBenchmarkOverride(string code, string label)
    {
        return new GlobalBenchmarkOverride(
            code,
            label,
            0,
            "",
            0,
            "",
            0,
            0.0,
            0,
            "",
            GlobalBenchmarkOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBenchmarkOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBenchmarkOverrideStatus next)
    {
        switch (Status)
        {
            case GlobalBenchmarkOverrideStatus.Open:
                switch (next)
                {
                    case GlobalBenchmarkOverrideStatus.InReview:
                    case GlobalBenchmarkOverrideStatus.Archived:
                        return true;
                }
                return false;
            case GlobalBenchmarkOverrideStatus.InReview:
                switch (next)
                {
                    case GlobalBenchmarkOverrideStatus.Resolved:
                    case GlobalBenchmarkOverrideStatus.Archived:
                        return true;
                }
                return false;
            case GlobalBenchmarkOverrideStatus.Resolved:
                switch (next)
                {
                    case GlobalBenchmarkOverrideStatus.Archived:
                        return true;
                }
                return false;
            case GlobalBenchmarkOverrideStatus.Reopened:
                switch (next)
                {
                    case GlobalBenchmarkOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalBenchmarkOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global benchmark override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global benchmark override"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)CreatedAt + (double)GraceDays, 2);
    }

    public GlobalBenchmarkOverride Copy()
    {
        return new GlobalBenchmarkOverride(
            Code,
            Label,
            Quantity,
            CostCenter,
            CreatedAt,
            Category,
            Attempts,
            Weight,
            GraceDays,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
