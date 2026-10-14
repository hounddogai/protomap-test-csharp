namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor quota is in its life cycle.</summary>
public enum VendorQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One vendor quota, with what the planners need to act on it.</summary>
public sealed class VendorQuota(string code, string title, string segment, string ownerId, double hours, VendorQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string OwnerId { get; set; } = ownerId;
    public double Hours { get; set; } = hours;
    public VendorQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a vendor quota in status proposed.</summary>
    public static VendorQuota NewVendorQuota(string code, string title)
    {
        return new VendorQuota(code, title, "", "", 0.0, VendorQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorQuotaStatus next)
    {
        switch (Status)
        {
            case VendorQuotaStatus.Proposed:
                return next == VendorQuotaStatus.Approved;
            case VendorQuotaStatus.Approved:
                switch (next)
                {
                    case VendorQuotaStatus.Rejected:
                    case VendorQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case VendorQuotaStatus.Rejected:
                switch (next)
                {
                    case VendorQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public VendorQuota Copy()
    {
        return new VendorQuota(Code, Title, Segment, OwnerId, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
