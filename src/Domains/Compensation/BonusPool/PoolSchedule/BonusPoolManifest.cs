namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool manifest is in its life cycle.</summary>
public enum BonusPoolManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a bonus pool manifest and its current status.</summary>
public sealed class BonusPoolManifest(string code, string title, double maximum, string jobCode, double ceiling, int tenureMonths, string source, int retries, string region, int durationMinutes, int attempts, BonusPoolManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public double Ceiling { get; set; } = ceiling;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Attempts { get; set; } = attempts;
    public BonusPoolManifestStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool manifest in status draft.</summary>
    public static BonusPoolManifest NewBonusPoolManifest(string code, string title)
    {
        return new BonusPoolManifest(code, title, 0.0, "", 0.0, 0, "", 0, "", 0, 0, BonusPoolManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolManifestStatus next)
    {
        switch (Status)
        {
            case BonusPoolManifestStatus.Draft:
                return next == BonusPoolManifestStatus.Suspended;
            case BonusPoolManifestStatus.Pending:
                switch (next)
                {
                    case BonusPoolManifestStatus.Active:
                    case BonusPoolManifestStatus.Closed:
                        return true;
                }
                return false;
            case BonusPoolManifestStatus.Active:
                return next == BonusPoolManifestStatus.Closed;
            case BonusPoolManifestStatus.Suspended:
                switch (next)
                {
                    case BonusPoolManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"bonus pool manifest"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Maximum * (double)TenureMonths, 2);
    }

    public BonusPoolManifest Copy()
    {
        return new BonusPoolManifest(
            Code,
            Title,
            Maximum,
            JobCode,
            Ceiling,
            TenureMonths,
            Source,
            Retries,
            Region,
            DurationMinutes,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
