namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor window is in its life cycle.</summary>
public enum VendorWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A vendor window. Its code is unique within a tenant.</summary>
public sealed class VendorWindow(string code, string label, string country, double floorValue, double minimum, int quantity, int sequence, string name, int limit, VendorWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public VendorWindowStatus Status { get; set; } = status;

    /// <summary>Starts a vendor window in status planned.</summary>
    public static VendorWindow NewVendorWindow(string code, string label)
    {
        return new VendorWindow(code, label, "", 0.0, 0.0, 0, 0, "", 0, VendorWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorWindowStatus next)
    {
        switch (Status)
        {
            case VendorWindowStatus.Planned:
                switch (next)
                {
                    case VendorWindowStatus.Scheduled:
                    case VendorWindowStatus.InProgress:
                        return true;
                }
                return false;
            case VendorWindowStatus.Scheduled:
                switch (next)
                {
                    case VendorWindowStatus.Completed:
                        return true;
                }
                return false;
            case VendorWindowStatus.InProgress:
                switch (next)
                {
                    case VendorWindowStatus.Completed:
                    case VendorWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorWindowStatus.Completed:
                switch (next)
                {
                    case VendorWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"vendor window"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Minimum * (double)Quantity, 2);
    }

    public VendorWindow Copy()
    {
        return new VendorWindow(Code, Label, Country, FloorValue, Minimum, Quantity, Sequence, Name, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
