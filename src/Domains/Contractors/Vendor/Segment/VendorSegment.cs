namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor segment is in its life cycle.</summary>
public enum VendorSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a vendor segment and its current status.</summary>
public sealed class VendorSegment(string code, string title, string ownerId, string currency, int graceDays, int leaveDays, double ceiling, VendorSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ceiling { get; set; } = ceiling;
    public VendorSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a vendor segment in status draft.</summary>
    public static VendorSegment NewVendorSegment(string code, string title)
    {
        return new VendorSegment(code, title, "", "", 0, 0, 0.0, VendorSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorSegmentStatus next)
    {
        switch (Status)
        {
            case VendorSegmentStatus.Draft:
                switch (next)
                {
                    case VendorSegmentStatus.Suspended:
                        return true;
                }
                return false;
            case VendorSegmentStatus.Pending:
                switch (next)
                {
                    case VendorSegmentStatus.Closed:
                        return true;
                }
                return false;
            case VendorSegmentStatus.Active:
                return next == VendorSegmentStatus.Suspended || next == VendorSegmentStatus.Closed;
            case VendorSegmentStatus.Suspended:
                switch (next)
                {
                    case VendorSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays - Ceiling, 2);
    }

    public VendorSegment Copy()
    {
        return new VendorSegment(Code, Title, OwnerId, Currency, GraceDays, LeaveDays, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
