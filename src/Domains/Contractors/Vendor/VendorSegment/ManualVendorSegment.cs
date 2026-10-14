namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual vendor segment is in its life cycle.</summary>
public enum ManualVendorSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A manual vendor segment as the vendor team keeps it.</summary>
public sealed class ManualVendorSegment(string code, string title, string locale, string channel, int priority, string employeeId, string costCenter, double maximum, string notes, ManualVendorSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Channel { get; set; } = channel;
    public int Priority { get; set; } = priority;
    public string EmployeeId { get; set; } = employeeId;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public string Notes { get; set; } = notes;
    public ManualVendorSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual vendor segment in status proposed.</summary>
    public static ManualVendorSegment NewManualVendorSegment(string code, string title)
    {
        return new ManualVendorSegment(code, title, "", "", 0, "", "", 0.0, "", ManualVendorSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualVendorSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualVendorSegmentStatus next)
    {
        switch (Status)
        {
            case ManualVendorSegmentStatus.Proposed:
                return next == ManualVendorSegmentStatus.Approved;
            case ManualVendorSegmentStatus.Approved:
                return next == ManualVendorSegmentStatus.Withdrawn;
            case ManualVendorSegmentStatus.Rejected:
                return next == ManualVendorSegmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualVendorSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual vendor segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round(Maximum - (double)Priority, 2);
    }

    public ManualVendorSegment Copy()
    {
        return new ManualVendorSegment(
            Code,
            Title,
            Locale,
            Channel,
            Priority,
            EmployeeId,
            CostCenter,
            Maximum,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
