namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted benchmark baseline is in its life cycle.</summary>
public enum WeightedBenchmarkBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted benchmark baseline. Its code is unique within a tenant.</summary>
public sealed class WeightedBenchmarkBaseline(string code, string title, string costCenter, string skill, string segment, double ratio, bool pinned, double cost, int durationMinutes, int tenureMonths, WeightedBenchmarkBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public WeightedBenchmarkBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a weighted benchmark baseline in status new.</summary>
    public static WeightedBenchmarkBaseline NewWeightedBenchmarkBaseline(string code, string title)
    {
        return new WeightedBenchmarkBaseline(
            code,
            title,
            "",
            "",
            "",
            0.0,
            false,
            0.0,
            0,
            0,
            WeightedBenchmarkBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBenchmarkBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBenchmarkBaselineStatus next)
    {
        switch (Status)
        {
            case WeightedBenchmarkBaselineStatus.New:
                return next == WeightedBenchmarkBaselineStatus.Running;
            case WeightedBenchmarkBaselineStatus.Queued:
                switch (next)
                {
                    case WeightedBenchmarkBaselineStatus.Running:
                        return true;
                }
                return false;
            case WeightedBenchmarkBaselineStatus.Running:
                switch (next)
                {
                    case WeightedBenchmarkBaselineStatus.Done:
                    case WeightedBenchmarkBaselineStatus.Failed:
                        return true;
                }
                return false;
            case WeightedBenchmarkBaselineStatus.Done:
                return next == WeightedBenchmarkBaselineStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedBenchmarkBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted benchmark baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round(Ratio + (double)DurationMinutes, 2);
    }

    public WeightedBenchmarkBaseline Copy()
    {
        return new WeightedBenchmarkBaseline(
            Code,
            Title,
            CostCenter,
            Skill,
            Segment,
            Ratio,
            Pinned,
            Cost,
            DurationMinutes,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
