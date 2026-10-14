namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor profile is in its life cycle.</summary>
public enum VendorProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One vendor profile, with what the planners need to act on it.</summary>
public sealed class VendorProfile(string code, string label, string name, string region, double maximum, string phone, VendorProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public double Maximum { get; set; } = maximum;
    public string Phone { get; set; } = phone;
    public VendorProfileStatus Status { get; set; } = status;

    /// <summary>Starts a vendor profile in status proposed.</summary>
    public static VendorProfile NewVendorProfile(string code, string label)
    {
        return new VendorProfile(code, label, "", "", 0.0, "", VendorProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorProfileStatus next)
    {
        switch (Status)
        {
            case VendorProfileStatus.Proposed:
                switch (next)
                {
                    case VendorProfileStatus.Approved:
                    case VendorProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case VendorProfileStatus.Approved:
                switch (next)
                {
                    case VendorProfileStatus.Rejected:
                    case VendorProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case VendorProfileStatus.Rejected:
                switch (next)
                {
                    case VendorProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public VendorProfile Copy()
    {
        return new VendorProfile(Code, Label, Name, Region, Maximum, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
