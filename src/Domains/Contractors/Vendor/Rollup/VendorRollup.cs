namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor rollup is in its life cycle.</summary>
public enum VendorRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A vendor rollup as the vendor team keeps it.</summary>
public sealed class VendorRollup(string code, string title, string department, string phone, int windowDays, string segment, VendorRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public VendorRollupStatus Status { get; set; } = status;

    /// <summary>Starts a vendor rollup in status idle.</summary>
    public static VendorRollup NewVendorRollup(string code, string title)
    {
        return new VendorRollup(code, title, "", "", 0, "", VendorRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorRollupStatus next)
    {
        switch (Status)
        {
            case VendorRollupStatus.Idle:
                return next == VendorRollupStatus.Draining || next == VendorRollupStatus.Stopped;
            case VendorRollupStatus.Warming:
                switch (next)
                {
                    case VendorRollupStatus.Ready:
                    case VendorRollupStatus.Draining:
                        return true;
                }
                return false;
            case VendorRollupStatus.Ready:
                switch (next)
                {
                    case VendorRollupStatus.Stopped:
                        return true;
                }
                return false;
            case VendorRollupStatus.Draining:
                return next == VendorRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(VendorRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor rollup"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public VendorRollup Copy()
    {
        return new VendorRollup(Code, Title, Department, Phone, WindowDays, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
