namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor tier is in its life cycle.</summary>
public enum VendorTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a vendor tier and its current status.</summary>
public sealed class VendorTier(string code, string name, double cost, string country, int version, double floorValue, double weight, string contactEmail, VendorTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public double Weight { get; set; } = weight;
    public string ContactEmail { get; set; } = contactEmail;
    public VendorTierStatus Status { get; set; } = status;

    /// <summary>Starts a vendor tier in status draft.</summary>
    public static VendorTier NewVendorTier(string code, string name)
    {
        return new VendorTier(code, name, 0.0, "", 0, 0.0, 0.0, "", VendorTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorTierStatus next)
    {
        switch (Status)
        {
            case VendorTierStatus.Draft:
                return next == VendorTierStatus.Pending || next == VendorTierStatus.Active;
            case VendorTierStatus.Pending:
                return next == VendorTierStatus.Suspended;
            case VendorTierStatus.Active:
                switch (next)
                {
                    case VendorTierStatus.Suspended:
                        return true;
                }
                return false;
            case VendorTierStatus.Suspended:
                switch (next)
                {
                    case VendorTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Weight + (double)Version, 2);
    }

    public VendorTier Copy()
    {
        return new VendorTier(Code, Name, Cost, Country, Version, FloorValue, Weight, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
