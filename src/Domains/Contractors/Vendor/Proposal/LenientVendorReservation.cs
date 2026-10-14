namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient vendor reservation is in its life cycle.</summary>
public enum LenientVendorReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient vendor reservation as the vendor team keeps it.</summary>
public sealed class LenientVendorReservation(string code, string name, int batchSize, string currency, string locale, bool pinned, string managerId, LenientVendorReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public bool Pinned { get; set; } = pinned;
    public string ManagerId { get; set; } = managerId;
    public LenientVendorReservationStatus Status { get; set; } = status;

    /// <summary>Starts a lenient vendor reservation in status created.</summary>
    public static LenientVendorReservation NewLenientVendorReservation(string code, string name)
    {
        return new LenientVendorReservation(
            code,
            name,
            0,
            "",
            "",
            false,
            "",
            LenientVendorReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVendorReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVendorReservationStatus next)
    {
        switch (Status)
        {
            case LenientVendorReservationStatus.Created:
                return next == LenientVendorReservationStatus.Validated || next == LenientVendorReservationStatus.Published;
            case LenientVendorReservationStatus.Validated:
                switch (next)
                {
                    case LenientVendorReservationStatus.Retired:
                        return true;
                }
                return false;
            case LenientVendorReservationStatus.Published:
                return next == LenientVendorReservationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientVendorReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient vendor reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public LenientVendorReservation Copy()
    {
        return new LenientVendorReservation(Code, Name, BatchSize, Currency, Locale, Pinned, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
