namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark forecast is in its life cycle.</summary>
public enum BenchmarkForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A benchmark forecast. Its code is unique within a tenant.</summary>
public sealed class BenchmarkForecast(string code, string label, int tenureMonths, double cost, int sequence, double ratio, int expiresAt, BenchmarkForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public double Ratio { get; set; } = ratio;
    public int ExpiresAt { get; set; } = expiresAt;
    public BenchmarkForecastStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark forecast in status proposed.</summary>
    public static BenchmarkForecast NewBenchmarkForecast(string code, string label)
    {
        return new BenchmarkForecast(code, label, 0, 0.0, 0, 0.0, 0, BenchmarkForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkForecastStatus next)
    {
        switch (Status)
        {
            case BenchmarkForecastStatus.Proposed:
                switch (next)
                {
                    case BenchmarkForecastStatus.Approved:
                    case BenchmarkForecastStatus.Rejected:
                        return true;
                }
                return false;
            case BenchmarkForecastStatus.Approved:
                switch (next)
                {
                    case BenchmarkForecastStatus.Rejected:
                    case BenchmarkForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case BenchmarkForecastStatus.Rejected:
                switch (next)
                {
                    case BenchmarkForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"benchmark forecast"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Sequence * (double)TenureMonths, 2);
    }

    public BenchmarkForecast Copy()
    {
        return new BenchmarkForecast(Code, Label, TenureMonths, Cost, Sequence, Ratio, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
