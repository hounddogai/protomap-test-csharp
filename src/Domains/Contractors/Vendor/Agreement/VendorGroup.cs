namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor group is in its life cycle.</summary>
public enum VendorGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A vendor group. Its code is unique within a tenant.</summary>
public sealed class VendorGroup(string code, string title, string managerId, string phone, int windowDays, double ratio, double margin, Dictionary<string, string> attributes, string region, double amount, double hours, VendorGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Phone { get; set; } = phone;
    public int WindowDays { get; set; } = windowDays;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Region { get; set; } = region;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public VendorGroupStatus Status { get; set; } = status;

    /// <summary>Starts a vendor group in status proposed.</summary>
    public static VendorGroup NewVendorGroup(string code, string title)
    {
        return new VendorGroup(
            code,
            title,
            "",
            "",
            0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            VendorGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorGroupStatus next)
    {
        switch (Status)
        {
            case VendorGroupStatus.Proposed:
                switch (next)
                {
                    case VendorGroupStatus.Approved:
                    case VendorGroupStatus.Rejected:
                        return true;
                }
                return false;
            case VendorGroupStatus.Approved:
                switch (next)
                {
                    case VendorGroupStatus.Withdrawn:
                        return true;
                }
                return false;
            case VendorGroupStatus.Rejected:
                switch (next)
                {
                    case VendorGroupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours * Ratio, 2);
    }

    public VendorGroup Copy()
    {
        return new VendorGroup(
            Code,
            Title,
            ManagerId,
            Phone,
            WindowDays,
            Ratio,
            Margin,
            Attributes,
            Region,
            Amount,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
