namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark mapping is in its life cycle.</summary>
public enum BenchmarkMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A benchmark mapping as the benchmark team keeps it.</summary>
public sealed class BenchmarkMapping(string code, string name, string notes, double maximum, double weight, string title, string salaryBand, double rate, double budget, string managerId, int endsAt, BenchmarkMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Maximum { get; set; } = maximum;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rate { get; set; } = rate;
    public double Budget { get; set; } = budget;
    public string ManagerId { get; set; } = managerId;
    public int EndsAt { get; set; } = endsAt;
    public BenchmarkMappingStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark mapping in status idle.</summary>
    public static BenchmarkMapping NewBenchmarkMapping(string code, string name)
    {
        return new BenchmarkMapping(code, name, "", 0.0, 0.0, "", "", 0.0, 0.0, "", 0, BenchmarkMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkMappingStatus next)
    {
        switch (Status)
        {
            case BenchmarkMappingStatus.Idle:
                switch (next)
                {
                    case BenchmarkMappingStatus.Warming:
                    case BenchmarkMappingStatus.Stopped:
                        return true;
                }
                return false;
            case BenchmarkMappingStatus.Warming:
                return next == BenchmarkMappingStatus.Ready;
            case BenchmarkMappingStatus.Ready:
                return next == BenchmarkMappingStatus.Draining || next == BenchmarkMappingStatus.Stopped;
            case BenchmarkMappingStatus.Draining:
                switch (next)
                {
                    case BenchmarkMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"benchmark mapping"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Budget * Rate, 2);
    }

    public BenchmarkMapping Copy()
    {
        return new BenchmarkMapping(
            Code,
            Name,
            Notes,
            Maximum,
            Weight,
            Title,
            SalaryBand,
            Rate,
            Budget,
            ManagerId,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
