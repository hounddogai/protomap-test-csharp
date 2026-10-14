namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay band cycle is in its life cycle.</summary>
public enum SeasonalPayBandCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal pay band cycle. Its code is unique within a tenant.</summary>
public sealed class SeasonalPayBandCycle(string code, string label, double margin, int graceDays, int windowDays, List<string> tags, string checksum, string segment, int capacity, int durationMinutes, SeasonalPayBandCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public List<string> Tags { get; set; } = tags;
    public string Checksum { get; set; } = checksum;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public SeasonalPayBandCycleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay band cycle in status proposed.</summary>
    public static SeasonalPayBandCycle NewSeasonalPayBandCycle(string code, string label)
    {
        return new SeasonalPayBandCycle(
            code,
            label,
            0.0,
            0,
            0,
            new List<string>(),
            "",
            "",
            0,
            0,
            SeasonalPayBandCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayBandCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayBandCycleStatus next)
    {
        switch (Status)
        {
            case SeasonalPayBandCycleStatus.Proposed:
                switch (next)
                {
                    case SeasonalPayBandCycleStatus.Approved:
                    case SeasonalPayBandCycleStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalPayBandCycleStatus.Approved:
                switch (next)
                {
                    case SeasonalPayBandCycleStatus.Rejected:
                    case SeasonalPayBandCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalPayBandCycleStatus.Rejected:
                switch (next)
                {
                    case SeasonalPayBandCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayBandCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay band cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Capacity * (double)DurationMinutes, 2);
    }

    public SeasonalPayBandCycle Copy()
    {
        return new SeasonalPayBandCycle(
            Code,
            Label,
            Margin,
            GraceDays,
            WindowDays,
            Tags,
            Checksum,
            Segment,
            Capacity,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
