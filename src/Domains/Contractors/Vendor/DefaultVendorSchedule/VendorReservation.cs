namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor reservation is in its life cycle.</summary>
public enum VendorReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A vendor reservation. Its code is unique within a tenant.</summary>
public sealed class VendorReservation(string code, string title, int limit, string notes, string region, VendorReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public VendorReservationStatus Status { get; set; } = status;

    /// <summary>Starts a vendor reservation in status draft.</summary>
    public static VendorReservation NewVendorReservation(string code, string title)
    {
        return new VendorReservation(code, title, 0, "", "", VendorReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorReservationStatus next)
    {
        switch (Status)
        {
            case VendorReservationStatus.Draft:
                switch (next)
                {
                    case VendorReservationStatus.Active:
                        return true;
                }
                return false;
            case VendorReservationStatus.Pending:
                switch (next)
                {
                    case VendorReservationStatus.Closed:
                        return true;
                }
                return false;
            case VendorReservationStatus.Active:
                switch (next)
                {
                    case VendorReservationStatus.Closed:
                        return true;
                }
                return false;
            case VendorReservationStatus.Suspended:
                return next == VendorReservationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(VendorReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor reservation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public VendorReservation Copy()
    {
        return new VendorReservation(Code, Title, Limit, Notes, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
