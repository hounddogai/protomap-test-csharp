namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor digest is in its life cycle.</summary>
public enum VendorDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a vendor digest and its current status.</summary>
public sealed class VendorDigest(string code, string name, string phone, string costCenter, bool archived, string label, string department, string summary, int windowDays, int tenureMonths, double fte, VendorDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public bool Archived { get; set; } = archived;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Fte { get; set; } = fte;
    public VendorDigestStatus Status { get; set; } = status;

    /// <summary>Starts a vendor digest in status new.</summary>
    public static VendorDigest NewVendorDigest(string code, string name)
    {
        return new VendorDigest(code, name, "", "", false, "", "", "", 0, 0, 0.0, VendorDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorDigestStatus next)
    {
        switch (Status)
        {
            case VendorDigestStatus.New:
                switch (next)
                {
                    case VendorDigestStatus.Running:
                    case VendorDigestStatus.Failed:
                        return true;
                }
                return false;
            case VendorDigestStatus.Queued:
                switch (next)
                {
                    case VendorDigestStatus.Running:
                    case VendorDigestStatus.Done:
                        return true;
                }
                return false;
            case VendorDigestStatus.Running:
                switch (next)
                {
                    case VendorDigestStatus.Done:
                        return true;
                }
                return false;
            case VendorDigestStatus.Done:
                return next == VendorDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VendorDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays - Fte, 2);
    }

    public VendorDigest Copy()
    {
        return new VendorDigest(
            Code,
            Name,
            Phone,
            CostCenter,
            Archived,
            Label,
            Department,
            Summary,
            WindowDays,
            TenureMonths,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
