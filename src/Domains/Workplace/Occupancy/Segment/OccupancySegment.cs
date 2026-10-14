namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy segment is in its life cycle.</summary>
public enum OccupancySegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A occupancy segment as the occupancy team keeps it.</summary>
public sealed class OccupancySegment(string code, string label, int windowDays, string notes, string summary, string skill, double discount, double weight, int createdAt, double ratio, string costCenter, OccupancySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public OccupancySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy segment in status created.</summary>
    public static OccupancySegment NewOccupancySegment(string code, string label)
    {
        return new OccupancySegment(
            code,
            label,
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            0.0,
            "",
            OccupancySegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancySegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancySegmentStatus next)
    {
        switch (Status)
        {
            case OccupancySegmentStatus.Created:
                return next == OccupancySegmentStatus.Validated || next == OccupancySegmentStatus.Published;
            case OccupancySegmentStatus.Validated:
                switch (next)
                {
                    case OccupancySegmentStatus.Published:
                    case OccupancySegmentStatus.Retired:
                        return true;
                }
                return false;
            case OccupancySegmentStatus.Published:
                switch (next)
                {
                    case OccupancySegmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays + (double)CreatedAt, 2);
    }

    public OccupancySegment Copy()
    {
        return new OccupancySegment(
            Code,
            Label,
            WindowDays,
            Notes,
            Summary,
            Skill,
            Discount,
            Weight,
            CreatedAt,
            Ratio,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
