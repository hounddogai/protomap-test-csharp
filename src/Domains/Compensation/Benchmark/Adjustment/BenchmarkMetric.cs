namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark metric is in its life cycle.</summary>
public enum BenchmarkMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a benchmark metric and its current status.</summary>
public sealed class BenchmarkMetric(string code, string title, string name, double maximum, double ceiling, double rating, string location, int windowDays, string phone, string badge, int expiresAt, BenchmarkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public double Rating { get; set; } = rating;
    public string Location { get; set; } = location;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public string Badge { get; set; } = badge;
    public int ExpiresAt { get; set; } = expiresAt;
    public BenchmarkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark metric in status draft.</summary>
    public static BenchmarkMetric NewBenchmarkMetric(string code, string title)
    {
        return new BenchmarkMetric(code, title, "", 0.0, 0.0, 0.0, "", 0, "", "", 0, BenchmarkMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkMetricStatus next)
    {
        switch (Status)
        {
            case BenchmarkMetricStatus.Draft:
                switch (next)
                {
                    case BenchmarkMetricStatus.Closed:
                        return true;
                }
                return false;
            case BenchmarkMetricStatus.Pending:
                return next == BenchmarkMetricStatus.Closed;
            case BenchmarkMetricStatus.Active:
                return next == BenchmarkMetricStatus.Closed;
            case BenchmarkMetricStatus.Suspended:
                switch (next)
                {
                    case BenchmarkMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating + Ceiling, 2);
    }

    public BenchmarkMetric Copy()
    {
        return new BenchmarkMetric(
            Code,
            Title,
            Name,
            Maximum,
            Ceiling,
            Rating,
            Location,
            WindowDays,
            Phone,
            Badge,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
