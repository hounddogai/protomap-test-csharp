namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted benchmark mapping is in its life cycle.</summary>
public enum WeightedBenchmarkMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a weighted benchmark mapping and its current status.</summary>
public sealed class WeightedBenchmarkMapping(string code, string title, double threshold, int graceDays, string country, string ownerId, WeightedBenchmarkMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public string OwnerId { get; set; } = ownerId;
    public WeightedBenchmarkMappingStatus Status { get; set; } = status;

    /// <summary>Starts a weighted benchmark mapping in status idle.</summary>
    public static WeightedBenchmarkMapping NewWeightedBenchmarkMapping(string code, string title)
    {
        return new WeightedBenchmarkMapping(code, title, 0.0, 0, "", "", WeightedBenchmarkMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBenchmarkMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBenchmarkMappingStatus next)
    {
        switch (Status)
        {
            case WeightedBenchmarkMappingStatus.Idle:
                return next == WeightedBenchmarkMappingStatus.Warming || next == WeightedBenchmarkMappingStatus.Draining;
            case WeightedBenchmarkMappingStatus.Warming:
                return next == WeightedBenchmarkMappingStatus.Draining || next == WeightedBenchmarkMappingStatus.Stopped;
            case WeightedBenchmarkMappingStatus.Ready:
                switch (next)
                {
                    case WeightedBenchmarkMappingStatus.Draining:
                    case WeightedBenchmarkMappingStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedBenchmarkMappingStatus.Draining:
                switch (next)
                {
                    case WeightedBenchmarkMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBenchmarkMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted benchmark mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)GraceDays + Threshold, 2);
    }

    public WeightedBenchmarkMapping Copy()
    {
        return new WeightedBenchmarkMapping(Code, Title, Threshold, GraceDays, Country, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
