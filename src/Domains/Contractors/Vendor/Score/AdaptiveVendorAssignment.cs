namespace Acme.Workforce.Contractors.Vendor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive vendor assignment is in its life cycle.</summary>
public enum AdaptiveVendorAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a adaptive vendor assignment and its current status.</summary>
public sealed class AdaptiveVendorAssignment(string code, string title, double margin, string phone, string externalRef, int limit, int quantity, string department, int startsAt, double ratio, AdaptiveVendorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public string ExternalRef { get; set; } = externalRef;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public double Ratio { get; set; } = ratio;
    public AdaptiveVendorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive vendor assignment in status proposed.</summary>
    public static AdaptiveVendorAssignment NewAdaptiveVendorAssignment(string code, string title)
    {
        return new AdaptiveVendorAssignment(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0,
            "",
            0,
            0.0,
            AdaptiveVendorAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveVendorAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveVendorAssignmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveVendorAssignmentStatus.Proposed:
                return next == AdaptiveVendorAssignmentStatus.Rejected;
            case AdaptiveVendorAssignmentStatus.Approved:
                switch (next)
                {
                    case AdaptiveVendorAssignmentStatus.Rejected:
                    case AdaptiveVendorAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdaptiveVendorAssignmentStatus.Rejected:
                return next == AdaptiveVendorAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveVendorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive vendor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin - (double)Quantity, 2);
    }

    public AdaptiveVendorAssignment Copy()
    {
        return new AdaptiveVendorAssignment(
            Code,
            Title,
            Margin,
            Phone,
            ExternalRef,
            Limit,
            Quantity,
            Department,
            StartsAt,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
