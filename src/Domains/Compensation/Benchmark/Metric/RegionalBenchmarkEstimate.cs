namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional benchmark estimate is in its life cycle.</summary>
public enum RegionalBenchmarkEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One regional benchmark estimate, with what the planners need to act on it.</summary>
public sealed class RegionalBenchmarkEstimate(string code, string name, string unit, double margin, string skill, RegionalBenchmarkEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Margin { get; set; } = margin;
    public string Skill { get; set; } = skill;
    public RegionalBenchmarkEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a regional benchmark estimate in status requested.</summary>
    public static RegionalBenchmarkEstimate NewRegionalBenchmarkEstimate(string code, string name)
    {
        return new RegionalBenchmarkEstimate(code, name, "", 0.0, "", RegionalBenchmarkEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBenchmarkEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBenchmarkEstimateStatus next)
    {
        switch (Status)
        {
            case RegionalBenchmarkEstimateStatus.Requested:
                return next == RegionalBenchmarkEstimateStatus.Granted || next == RegionalBenchmarkEstimateStatus.Revoked;
            case RegionalBenchmarkEstimateStatus.Granted:
                switch (next)
                {
                    case RegionalBenchmarkEstimateStatus.Revoked:
                        return true;
                }
                return false;
            case RegionalBenchmarkEstimateStatus.Revoked:
                return next == RegionalBenchmarkEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalBenchmarkEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional benchmark estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public RegionalBenchmarkEstimate Copy()
    {
        return new RegionalBenchmarkEstimate(Code, Name, Unit, Margin, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
