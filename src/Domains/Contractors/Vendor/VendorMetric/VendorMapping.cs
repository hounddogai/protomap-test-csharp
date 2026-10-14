namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor mapping is in its life cycle.</summary>
public enum VendorMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One vendor mapping, with what the planners need to act on it.</summary>
public sealed class VendorMapping(string code, string title, double fte, bool visible, string skill, VendorMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public VendorMappingStatus Status { get; set; } = status;

    /// <summary>Starts a vendor mapping in status proposed.</summary>
    public static VendorMapping NewVendorMapping(string code, string title)
    {
        return new VendorMapping(code, title, 0.0, false, "", VendorMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorMappingStatus next)
    {
        switch (Status)
        {
            case VendorMappingStatus.Proposed:
                return next == VendorMappingStatus.Rejected || next == VendorMappingStatus.Withdrawn;
            case VendorMappingStatus.Approved:
                switch (next)
                {
                    case VendorMappingStatus.Rejected:
                        return true;
                }
                return false;
            case VendorMappingStatus.Rejected:
                switch (next)
                {
                    case VendorMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public VendorMapping Copy()
    {
        return new VendorMapping(Code, Title, Fte, Visible, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
