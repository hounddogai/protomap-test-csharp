namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor policy is in its life cycle.</summary>
public enum VendorPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A vendor policy as the vendor team keeps it.</summary>
public sealed class VendorPolicy(string code, string label, int expiresAt, string title, string country, double weight, int version, string department, string salaryBand, VendorPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public double Weight { get; set; } = weight;
    public int Version { get; set; } = version;
    public string Department { get; set; } = department;
    public string SalaryBand { get; set; } = salaryBand;
    public VendorPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a vendor policy in status open.</summary>
    public static VendorPolicy NewVendorPolicy(string code, string label)
    {
        return new VendorPolicy(code, label, 0, "", "", 0.0, 0, "", "", VendorPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorPolicyStatus next)
    {
        switch (Status)
        {
            case VendorPolicyStatus.Open:
                switch (next)
                {
                    case VendorPolicyStatus.Reopened:
                    case VendorPolicyStatus.Archived:
                        return true;
                }
                return false;
            case VendorPolicyStatus.InReview:
                return next == VendorPolicyStatus.Resolved;
            case VendorPolicyStatus.Resolved:
                switch (next)
                {
                    case VendorPolicyStatus.Archived:
                        return true;
                }
                return false;
            case VendorPolicyStatus.Reopened:
                return next == VendorPolicyStatus.Archived;
        }
        return false;
    }

    public void MoveTo(VendorPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight * (double)Version, 2);
    }

    public VendorPolicy Copy()
    {
        return new VendorPolicy(
            Code,
            Label,
            ExpiresAt,
            Title,
            Country,
            Weight,
            Version,
            Department,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
