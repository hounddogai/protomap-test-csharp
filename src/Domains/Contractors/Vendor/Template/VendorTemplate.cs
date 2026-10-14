namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor template is in its life cycle.</summary>
public enum VendorTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A vendor template. Its code is unique within a tenant.</summary>
public sealed class VendorTemplate(string code, string name, string location, int version, string ownerId, string description, double weight, VendorTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int Version { get; set; } = version;
    public string OwnerId { get; set; } = ownerId;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public VendorTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a vendor template in status open.</summary>
    public static VendorTemplate NewVendorTemplate(string code, string name)
    {
        return new VendorTemplate(code, name, "", 0, "", "", 0.0, VendorTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorTemplateStatus next)
    {
        switch (Status)
        {
            case VendorTemplateStatus.Open:
                switch (next)
                {
                    case VendorTemplateStatus.Resolved:
                    case VendorTemplateStatus.Reopened:
                        return true;
                }
                return false;
            case VendorTemplateStatus.InReview:
                return next == VendorTemplateStatus.Reopened || next == VendorTemplateStatus.Archived;
            case VendorTemplateStatus.Resolved:
                return next == VendorTemplateStatus.Reopened || next == VendorTemplateStatus.Archived;
            case VendorTemplateStatus.Reopened:
                switch (next)
                {
                    case VendorTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Version + Weight, 2);
    }

    public VendorTemplate Copy()
    {
        return new VendorTemplate(Code, Name, Location, Version, OwnerId, Description, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
