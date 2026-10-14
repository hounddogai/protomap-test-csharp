namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle segment is in its life cycle.</summary>
public enum MeritCycleSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One merit cycle segment, with what the planners need to act on it.</summary>
public sealed class MeritCycleSegment(string code, string title, string description, int expiresAt, string location, int graceDays, double rating, MeritCycleSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public MeritCycleSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle segment in status draft.</summary>
    public static MeritCycleSegment NewMeritCycleSegment(string code, string title)
    {
        return new MeritCycleSegment(code, title, "", 0, "", 0, 0.0, MeritCycleSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleSegmentStatus next)
    {
        switch (Status)
        {
            case MeritCycleSegmentStatus.Draft:
                switch (next)
                {
                    case MeritCycleSegmentStatus.Active:
                    case MeritCycleSegmentStatus.Closed:
                        return true;
                }
                return false;
            case MeritCycleSegmentStatus.Pending:
                switch (next)
                {
                    case MeritCycleSegmentStatus.Active:
                    case MeritCycleSegmentStatus.Closed:
                        return true;
                }
                return false;
            case MeritCycleSegmentStatus.Active:
                switch (next)
                {
                    case MeritCycleSegmentStatus.Suspended:
                    case MeritCycleSegmentStatus.Closed:
                        return true;
                }
                return false;
            case MeritCycleSegmentStatus.Suspended:
                return next == MeritCycleSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MeritCycleSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rating - (double)ExpiresAt, 2);
    }

    public MeritCycleSegment Copy()
    {
        return new MeritCycleSegment(Code, Title, Description, ExpiresAt, Location, GraceDays, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
