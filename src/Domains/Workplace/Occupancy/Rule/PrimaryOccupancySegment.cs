namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary occupancy segment is in its life cycle.</summary>
public enum PrimaryOccupancySegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary occupancy segment as the occupancy team keeps it.</summary>
public sealed class PrimaryOccupancySegment(string code, string label, int updatedAt, bool visible, int windowDays, string country, double hours, Dictionary<string, string> attributes, int startsAt, double budget, string jobCode, PrimaryOccupancySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public int WindowDays { get; set; } = windowDays;
    public string Country { get; set; } = country;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public PrimaryOccupancySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary occupancy segment in status proposed.</summary>
    public static PrimaryOccupancySegment NewPrimaryOccupancySegment(string code, string label)
    {
        return new PrimaryOccupancySegment(
            code,
            label,
            0,
            false,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            0.0,
            "",
            PrimaryOccupancySegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOccupancySegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOccupancySegmentStatus next)
    {
        switch (Status)
        {
            case PrimaryOccupancySegmentStatus.Proposed:
                return next == PrimaryOccupancySegmentStatus.Approved;
            case PrimaryOccupancySegmentStatus.Approved:
                return next == PrimaryOccupancySegmentStatus.Rejected || next == PrimaryOccupancySegmentStatus.Withdrawn;
            case PrimaryOccupancySegmentStatus.Rejected:
                return next == PrimaryOccupancySegmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PrimaryOccupancySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary occupancy segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedValue()
    {
        return Math.Round((double)StartsAt * (double)WindowDays, 2);
    }

    public PrimaryOccupancySegment Copy()
    {
        return new PrimaryOccupancySegment(
            Code,
            Label,
            UpdatedAt,
            Visible,
            WindowDays,
            Country,
            Hours,
            Attributes,
            StartsAt,
            Budget,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
