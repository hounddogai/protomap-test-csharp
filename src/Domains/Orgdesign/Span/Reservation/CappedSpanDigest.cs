namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped span digest is in its life cycle.</summary>
public enum CappedSpanDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a capped span digest and its current status.</summary>
public sealed class CappedSpanDigest(string code, string title, double floorValue, int limit, double minimum, int leaveDays, int windowDays, string department, string name, double rating, CappedSpanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public int WindowDays { get; set; } = windowDays;
    public string Department { get; set; } = department;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public CappedSpanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a capped span digest in status draft.</summary>
    public static CappedSpanDigest NewCappedSpanDigest(string code, string title)
    {
        return new CappedSpanDigest(code, title, 0.0, 0, 0.0, 0, 0, "", "", 0.0, CappedSpanDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSpanDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSpanDigestStatus next)
    {
        switch (Status)
        {
            case CappedSpanDigestStatus.Draft:
                switch (next)
                {
                    case CappedSpanDigestStatus.Active:
                    case CappedSpanDigestStatus.Suspended:
                        return true;
                }
                return false;
            case CappedSpanDigestStatus.Pending:
                switch (next)
                {
                    case CappedSpanDigestStatus.Active:
                        return true;
                }
                return false;
            case CappedSpanDigestStatus.Active:
                switch (next)
                {
                    case CappedSpanDigestStatus.Suspended:
                        return true;
                }
                return false;
            case CappedSpanDigestStatus.Suspended:
                return next == CappedSpanDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedSpanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped span digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rating - Minimum, 2);
    }

    public CappedSpanDigest Copy()
    {
        return new CappedSpanDigest(
            Code,
            Title,
            FloorValue,
            Limit,
            Minimum,
            LeaveDays,
            WindowDays,
            Department,
            Name,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
