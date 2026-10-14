namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal benchmark digest is in its life cycle.</summary>
public enum SeasonalBenchmarkDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal benchmark digest. Its code is unique within a tenant.</summary>
public sealed class SeasonalBenchmarkDigest(string code, string label, string timezone, double rate, int startsAt, string badge, int leaveDays, SeasonalBenchmarkDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public string Badge { get; set; } = badge;
    public int LeaveDays { get; set; } = leaveDays;
    public SeasonalBenchmarkDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal benchmark digest in status created.</summary>
    public static SeasonalBenchmarkDigest NewSeasonalBenchmarkDigest(string code, string label)
    {
        return new SeasonalBenchmarkDigest(code, label, "", 0.0, 0, "", 0, SeasonalBenchmarkDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBenchmarkDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBenchmarkDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalBenchmarkDigestStatus.Created:
                switch (next)
                {
                    case SeasonalBenchmarkDigestStatus.Validated:
                    case SeasonalBenchmarkDigestStatus.Published:
                        return true;
                }
                return false;
            case SeasonalBenchmarkDigestStatus.Validated:
                return next == SeasonalBenchmarkDigestStatus.Published || next == SeasonalBenchmarkDigestStatus.Retired;
            case SeasonalBenchmarkDigestStatus.Published:
                switch (next)
                {
                    case SeasonalBenchmarkDigestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBenchmarkDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal benchmark digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"seasonal benchmark digest"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt * Rate, 2);
    }

    public SeasonalBenchmarkDigest Copy()
    {
        return new SeasonalBenchmarkDigest(Code, Label, Timezone, Rate, StartsAt, Badge, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
