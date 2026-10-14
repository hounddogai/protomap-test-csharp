namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped laptop segment is in its life cycle.</summary>
public enum CappedLaptopSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped laptop segment, with what the planners need to act on it.</summary>
public sealed class CappedLaptopSegment(string code, string label, bool visible, double hours, double ratio, string channel, string country, string category, double rating, double cost, CappedLaptopSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public double Hours { get; set; } = hours;
    public double Ratio { get; set; } = ratio;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public double Rating { get; set; } = rating;
    public double Cost { get; set; } = cost;
    public CappedLaptopSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped laptop segment in status proposed.</summary>
    public static CappedLaptopSegment NewCappedLaptopSegment(string code, string label)
    {
        return new CappedLaptopSegment(
            code,
            label,
            false,
            0.0,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            CappedLaptopSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLaptopSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLaptopSegmentStatus next)
    {
        switch (Status)
        {
            case CappedLaptopSegmentStatus.Proposed:
                return next == CappedLaptopSegmentStatus.Approved;
            case CappedLaptopSegmentStatus.Approved:
                return next == CappedLaptopSegmentStatus.Rejected;
            case CappedLaptopSegmentStatus.Rejected:
                switch (next)
                {
                    case CappedLaptopSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedLaptopSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped laptop segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped laptop segment"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours - Ratio, 2);
    }

    public CappedLaptopSegment Copy()
    {
        return new CappedLaptopSegment(
            Code,
            Label,
            Visible,
            Hours,
            Ratio,
            Channel,
            Country,
            Category,
            Rating,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
