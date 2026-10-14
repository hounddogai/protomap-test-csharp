namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate mapping is in its life cycle.</summary>
public enum RateMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rate mapping and its current status.</summary>
public sealed class RateMapping(string code, string title, bool visible, int version, int durationMinutes, string phone, double rating, bool approved, RateMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public int Version { get; set; } = version;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public bool Approved { get; set; } = approved;
    public RateMappingStatus Status { get; set; } = status;

    /// <summary>Starts a rate mapping in status new.</summary>
    public static RateMapping NewRateMapping(string code, string title)
    {
        return new RateMapping(code, title, false, 0, 0, "", 0.0, false, RateMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateMappingStatus next)
    {
        switch (Status)
        {
            case RateMappingStatus.New:
                return next == RateMappingStatus.Done || next == RateMappingStatus.Failed;
            case RateMappingStatus.Queued:
                return next == RateMappingStatus.Running;
            case RateMappingStatus.Running:
                switch (next)
                {
                    case RateMappingStatus.Failed:
                        return true;
                }
                return false;
            case RateMappingStatus.Done:
                switch (next)
                {
                    case RateMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rating + (double)Version, 2);
    }

    public RateMapping Copy()
    {
        return new RateMapping(Code, Title, Visible, Version, DurationMinutes, Phone, Rating, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
