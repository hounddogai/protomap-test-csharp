namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark override is in its life cycle.</summary>
public enum BenchmarkOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One benchmark override, with what the planners need to act on it.</summary>
public sealed class BenchmarkOverride(string code, string title, int createdAt, string region, string unit, double rating, BenchmarkOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public string Unit { get; set; } = unit;
    public double Rating { get; set; } = rating;
    public BenchmarkOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark override in status idle.</summary>
    public static BenchmarkOverride NewBenchmarkOverride(string code, string title)
    {
        return new BenchmarkOverride(code, title, 0, "", "", 0.0, BenchmarkOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkOverrideStatus next)
    {
        switch (Status)
        {
            case BenchmarkOverrideStatus.Idle:
                return next == BenchmarkOverrideStatus.Warming;
            case BenchmarkOverrideStatus.Warming:
                return next == BenchmarkOverrideStatus.Ready || next == BenchmarkOverrideStatus.Draining;
            case BenchmarkOverrideStatus.Ready:
                return next == BenchmarkOverrideStatus.Draining || next == BenchmarkOverrideStatus.Stopped;
            case BenchmarkOverrideStatus.Draining:
                switch (next)
                {
                    case BenchmarkOverrideStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)CreatedAt - Rating, 2);
    }

    public BenchmarkOverride Copy()
    {
        return new BenchmarkOverride(Code, Title, CreatedAt, Region, Unit, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
