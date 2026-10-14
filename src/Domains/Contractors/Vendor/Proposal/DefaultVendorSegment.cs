namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default vendor segment is in its life cycle.</summary>
public enum DefaultVendorSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default vendor segment as the vendor team keeps it.</summary>
public sealed class DefaultVendorSegment(string code, string label, string phone, bool approved, int limit, double cost, double score, string segment, double floorValue, double hours, DefaultVendorSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public int Limit { get; set; } = limit;
    public double Cost { get; set; } = cost;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public DefaultVendorSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a default vendor segment in status idle.</summary>
    public static DefaultVendorSegment NewDefaultVendorSegment(string code, string label)
    {
        return new DefaultVendorSegment(
            code,
            label,
            "",
            false,
            0,
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            DefaultVendorSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVendorSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVendorSegmentStatus next)
    {
        switch (Status)
        {
            case DefaultVendorSegmentStatus.Idle:
                switch (next)
                {
                    case DefaultVendorSegmentStatus.Warming:
                    case DefaultVendorSegmentStatus.Draining:
                        return true;
                }
                return false;
            case DefaultVendorSegmentStatus.Warming:
                return next == DefaultVendorSegmentStatus.Ready || next == DefaultVendorSegmentStatus.Draining;
            case DefaultVendorSegmentStatus.Ready:
                return next == DefaultVendorSegmentStatus.Draining || next == DefaultVendorSegmentStatus.Stopped;
            case DefaultVendorSegmentStatus.Draining:
                return next == DefaultVendorSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultVendorSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default vendor segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Cost - Hours, 2);
    }

    public DefaultVendorSegment Copy()
    {
        return new DefaultVendorSegment(
            Code,
            Label,
            Phone,
            Approved,
            Limit,
            Cost,
            Score,
            Segment,
            FloorValue,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
