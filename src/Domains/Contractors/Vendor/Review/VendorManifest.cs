namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor manifest is in its life cycle.</summary>
public enum VendorManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A vendor manifest. Its code is unique within a tenant.</summary>
public sealed class VendorManifest(string code, string label, bool archived, string channel, double margin, double ratio, double threshold, string summary, string name, string phone, VendorManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public double Margin { get; set; } = margin;
    public double Ratio { get; set; } = ratio;
    public double Threshold { get; set; } = threshold;
    public string Summary { get; set; } = summary;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public VendorManifestStatus Status { get; set; } = status;

    /// <summary>Starts a vendor manifest in status proposed.</summary>
    public static VendorManifest NewVendorManifest(string code, string label)
    {
        return new VendorManifest(code, label, false, "", 0.0, 0.0, 0.0, "", "", "", VendorManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorManifestStatus next)
    {
        switch (Status)
        {
            case VendorManifestStatus.Proposed:
                switch (next)
                {
                    case VendorManifestStatus.Approved:
                    case VendorManifestStatus.Rejected:
                        return true;
                }
                return false;
            case VendorManifestStatus.Approved:
                return next == VendorManifestStatus.Withdrawn;
            case VendorManifestStatus.Rejected:
                switch (next)
                {
                    case VendorManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Threshold * Margin, 2);
    }

    public VendorManifest Copy()
    {
        return new VendorManifest(
            Code,
            Label,
            Archived,
            Channel,
            Margin,
            Ratio,
            Threshold,
            Summary,
            Name,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
