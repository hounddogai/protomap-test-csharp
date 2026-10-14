namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal mobility record is in its life cycle.</summary>
public enum SeasonalMobilityRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal mobility record. Its code is unique within a tenant.</summary>
public sealed class SeasonalMobilityRecord(string code, string label, int windowDays, double score, double rating, int startsAt, string grade, double ratio, string region, int revision, SeasonalMobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public double Rating { get; set; } = rating;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public int Revision { get; set; } = revision;
    public SeasonalMobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal mobility record in status planned.</summary>
    public static SeasonalMobilityRecord NewSeasonalMobilityRecord(string code, string label)
    {
        return new SeasonalMobilityRecord(
            code,
            label,
            0,
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            0,
            SeasonalMobilityRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMobilityRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMobilityRecordStatus next)
    {
        switch (Status)
        {
            case SeasonalMobilityRecordStatus.Planned:
                return next == SeasonalMobilityRecordStatus.Completed || next == SeasonalMobilityRecordStatus.Cancelled;
            case SeasonalMobilityRecordStatus.Scheduled:
                switch (next)
                {
                    case SeasonalMobilityRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalMobilityRecordStatus.InProgress:
                return next == SeasonalMobilityRecordStatus.Completed;
            case SeasonalMobilityRecordStatus.Completed:
                switch (next)
                {
                    case SeasonalMobilityRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalMobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Score * (double)WindowDays, 2);
    }

    public SeasonalMobilityRecord Copy()
    {
        return new SeasonalMobilityRecord(
            Code,
            Label,
            WindowDays,
            Score,
            Rating,
            StartsAt,
            Grade,
            Ratio,
            Region,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
