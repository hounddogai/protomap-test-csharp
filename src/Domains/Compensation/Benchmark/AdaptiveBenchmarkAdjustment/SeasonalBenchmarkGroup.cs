namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal benchmark group is in its life cycle.</summary>
public enum SeasonalBenchmarkGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a seasonal benchmark group and its current status.</summary>
public sealed class SeasonalBenchmarkGroup(string code, string name, string timezone, double ratio, string reviewer, string contactEmail, int capacity, string ownerId, int leaveDays, SeasonalBenchmarkGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public string Reviewer { get; set; } = reviewer;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public SeasonalBenchmarkGroupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal benchmark group in status created.</summary>
    public static SeasonalBenchmarkGroup NewSeasonalBenchmarkGroup(string code, string name)
    {
        return new SeasonalBenchmarkGroup(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            "",
            0,
            SeasonalBenchmarkGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBenchmarkGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBenchmarkGroupStatus next)
    {
        switch (Status)
        {
            case SeasonalBenchmarkGroupStatus.Created:
                return next == SeasonalBenchmarkGroupStatus.Validated || next == SeasonalBenchmarkGroupStatus.Published;
            case SeasonalBenchmarkGroupStatus.Validated:
                switch (next)
                {
                    case SeasonalBenchmarkGroupStatus.Published:
                    case SeasonalBenchmarkGroupStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalBenchmarkGroupStatus.Published:
                return next == SeasonalBenchmarkGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalBenchmarkGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal benchmark group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal benchmark group"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Capacity - Ratio, 2);
    }

    public SeasonalBenchmarkGroup Copy()
    {
        return new SeasonalBenchmarkGroup(
            Code,
            Name,
            Timezone,
            Ratio,
            Reviewer,
            ContactEmail,
            Capacity,
            OwnerId,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
