namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor override is in its life cycle.</summary>
public enum VendorOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A vendor override as the vendor team keeps it.</summary>
public sealed class VendorOverride(string code, string label, string title, string phone, string region, string checksum, string name, int createdAt, VendorOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public string Checksum { get; set; } = checksum;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public VendorOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a vendor override in status idle.</summary>
    public static VendorOverride NewVendorOverride(string code, string label)
    {
        return new VendorOverride(code, label, "", "", "", "", "", 0, VendorOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorOverrideStatus next)
    {
        switch (Status)
        {
            case VendorOverrideStatus.Idle:
                switch (next)
                {
                    case VendorOverrideStatus.Warming:
                    case VendorOverrideStatus.Draining:
                        return true;
                }
                return false;
            case VendorOverrideStatus.Warming:
                return next == VendorOverrideStatus.Ready || next == VendorOverrideStatus.Draining;
            case VendorOverrideStatus.Ready:
                return next == VendorOverrideStatus.Draining;
            case VendorOverrideStatus.Draining:
                switch (next)
                {
                    case VendorOverrideStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public VendorOverride Copy()
    {
        return new VendorOverride(Code, Label, Title, Phone, Region, Checksum, Name, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
