namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor threshold is in its life cycle.</summary>
public enum VendorThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a vendor threshold and its current status.</summary>
public sealed class VendorThreshold(string code, string name, double minimum, double hours, string unit, VendorThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public string Unit { get; set; } = unit;
    public VendorThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a vendor threshold in status planned.</summary>
    public static VendorThreshold NewVendorThreshold(string code, string name)
    {
        return new VendorThreshold(code, name, 0.0, 0.0, "", VendorThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorThresholdStatus next)
    {
        switch (Status)
        {
            case VendorThresholdStatus.Planned:
                switch (next)
                {
                    case VendorThresholdStatus.Completed:
                    case VendorThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorThresholdStatus.Scheduled:
                switch (next)
                {
                    case VendorThresholdStatus.Completed:
                        return true;
                }
                return false;
            case VendorThresholdStatus.InProgress:
                switch (next)
                {
                    case VendorThresholdStatus.Completed:
                    case VendorThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorThresholdStatus.Completed:
                return next == VendorThresholdStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(VendorThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor threshold"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Hours + Minimum, 2);
    }

    public VendorThreshold Copy()
    {
        return new VendorThreshold(Code, Name, Minimum, Hours, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
