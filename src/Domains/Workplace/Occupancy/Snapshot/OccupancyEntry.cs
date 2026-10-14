namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy entry is in its life cycle.</summary>
public enum OccupancyEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One occupancy entry, with what the planners need to act on it.</summary>
public sealed class OccupancyEntry(string code, string name, string jobCode, double maximum, double score, double fte, int windowDays, int startsAt, string source, double ratio, string contactEmail, OccupancyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Source { get; set; } = source;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public OccupancyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy entry in status planned.</summary>
    public static OccupancyEntry NewOccupancyEntry(string code, string name)
    {
        return new OccupancyEntry(code, name, "", 0.0, 0.0, 0.0, 0, 0, "", 0.0, "", OccupancyEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyEntryStatus next)
    {
        switch (Status)
        {
            case OccupancyEntryStatus.Planned:
                switch (next)
                {
                    case OccupancyEntryStatus.InProgress:
                    case OccupancyEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyEntryStatus.Scheduled:
                switch (next)
                {
                    case OccupancyEntryStatus.Completed:
                        return true;
                }
                return false;
            case OccupancyEntryStatus.InProgress:
                return next == OccupancyEntryStatus.Cancelled;
            case OccupancyEntryStatus.Completed:
                switch (next)
                {
                    case OccupancyEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio * (double)WindowDays, 2);
    }

    public OccupancyEntry Copy()
    {
        return new OccupancyEntry(
            Code,
            Name,
            JobCode,
            Maximum,
            Score,
            Fte,
            WindowDays,
            StartsAt,
            Source,
            Ratio,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
