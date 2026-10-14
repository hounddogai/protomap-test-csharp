namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient position segment is in its life cycle.</summary>
public enum LenientPositionSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient position segment as the position team keeps it.</summary>
public sealed class LenientPositionSegment(string code, string name, string department, string country, double discount, double fte, double margin, string team, int durationMinutes, LenientPositionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LenientPositionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient position segment in status draft.</summary>
    public static LenientPositionSegment NewLenientPositionSegment(string code, string name)
    {
        return new LenientPositionSegment(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            0,
            LenientPositionSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPositionSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPositionSegmentStatus next)
    {
        switch (Status)
        {
            case LenientPositionSegmentStatus.Draft:
                return next == LenientPositionSegmentStatus.Pending || next == LenientPositionSegmentStatus.Closed;
            case LenientPositionSegmentStatus.Pending:
                return next == LenientPositionSegmentStatus.Active || next == LenientPositionSegmentStatus.Suspended;
            case LenientPositionSegmentStatus.Active:
                return next == LenientPositionSegmentStatus.Suspended || next == LenientPositionSegmentStatus.Closed;
            case LenientPositionSegmentStatus.Suspended:
                return next == LenientPositionSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientPositionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient position segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Discount * Fte, 2);
    }

    public LenientPositionSegment Copy()
    {
        return new LenientPositionSegment(
            Code,
            Name,
            Department,
            Country,
            Discount,
            Fte,
            Margin,
            Team,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
