namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor bundle is in its life cycle.</summary>
public enum VendorBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A vendor bundle as the vendor team keeps it.</summary>
public sealed class VendorBundle(string code, string title, double rating, string skill, string location, int expiresAt, bool pinned, double hours, int leaveDays, VendorBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Skill { get; set; } = skill;
    public string Location { get; set; } = location;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public int LeaveDays { get; set; } = leaveDays;
    public VendorBundleStatus Status { get; set; } = status;

    /// <summary>Starts a vendor bundle in status idle.</summary>
    public static VendorBundle NewVendorBundle(string code, string title)
    {
        return new VendorBundle(code, title, 0.0, "", "", 0, false, 0.0, 0, VendorBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorBundleStatus next)
    {
        switch (Status)
        {
            case VendorBundleStatus.Idle:
                switch (next)
                {
                    case VendorBundleStatus.Stopped:
                        return true;
                }
                return false;
            case VendorBundleStatus.Warming:
                return next == VendorBundleStatus.Draining;
            case VendorBundleStatus.Ready:
                switch (next)
                {
                    case VendorBundleStatus.Draining:
                    case VendorBundleStatus.Stopped:
                        return true;
                }
                return false;
            case VendorBundleStatus.Draining:
                return next == VendorBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(VendorBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours + (double)ExpiresAt, 2);
    }

    public VendorBundle Copy()
    {
        return new VendorBundle(Code, Title, Rating, Skill, Location, ExpiresAt, Pinned, Hours, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
