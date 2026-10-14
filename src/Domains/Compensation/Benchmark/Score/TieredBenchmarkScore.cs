namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered benchmark score is in its life cycle.</summary>
public enum TieredBenchmarkScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tiered benchmark score, with what the planners need to act on it.</summary>
public sealed class TieredBenchmarkScore(string code, string name, double hours, int durationMinutes, double discount, string externalRef, string description, string jobCode, TieredBenchmarkScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public string Description { get; set; } = description;
    public string JobCode { get; set; } = jobCode;
    public TieredBenchmarkScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tiered benchmark score in status proposed.</summary>
    public static TieredBenchmarkScore NewTieredBenchmarkScore(string code, string name)
    {
        return new TieredBenchmarkScore(code, name, 0.0, 0, 0.0, "", "", "", TieredBenchmarkScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBenchmarkScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBenchmarkScoreStatus next)
    {
        switch (Status)
        {
            case TieredBenchmarkScoreStatus.Proposed:
                switch (next)
                {
                    case TieredBenchmarkScoreStatus.Approved:
                        return true;
                }
                return false;
            case TieredBenchmarkScoreStatus.Approved:
                return next == TieredBenchmarkScoreStatus.Rejected || next == TieredBenchmarkScoreStatus.Withdrawn;
            case TieredBenchmarkScoreStatus.Rejected:
                switch (next)
                {
                    case TieredBenchmarkScoreStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBenchmarkScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered benchmark score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Discount - Hours, 2);
    }

    public TieredBenchmarkScore Copy()
    {
        return new TieredBenchmarkScore(
            Code,
            Name,
            Hours,
            DurationMinutes,
            Discount,
            ExternalRef,
            Description,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
