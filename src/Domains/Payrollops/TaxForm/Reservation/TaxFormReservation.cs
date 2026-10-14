namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form reservation is in its life cycle.</summary>
public enum TaxFormReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tax form reservation as the tax form team keeps it.</summary>
public sealed class TaxFormReservation(string code, string label, double ceiling, string region, int endsAt, string category, string location, string checksum, bool pinned, int version, TaxFormReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Region { get; set; } = region;
    public int EndsAt { get; set; } = endsAt;
    public string Category { get; set; } = category;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public TaxFormReservationStatus Status { get; set; } = status;

    /// <summary>Starts a tax form reservation in status requested.</summary>
    public static TaxFormReservation NewTaxFormReservation(string code, string label)
    {
        return new TaxFormReservation(
            code,
            label,
            0.0,
            "",
            0,
            "",
            "",
            "",
            false,
            0,
            TaxFormReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormReservationStatus next)
    {
        switch (Status)
        {
            case TaxFormReservationStatus.Requested:
                return next == TaxFormReservationStatus.Granted || next == TaxFormReservationStatus.Expired;
            case TaxFormReservationStatus.Granted:
                switch (next)
                {
                    case TaxFormReservationStatus.Expired:
                        return true;
                }
                return false;
            case TaxFormReservationStatus.Revoked:
                return next == TaxFormReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TaxFormReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version + Ceiling, 2);
    }

    public TaxFormReservation Copy()
    {
        return new TaxFormReservation(
            Code,
            Label,
            Ceiling,
            Region,
            EndsAt,
            Category,
            Location,
            Checksum,
            Pinned,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
