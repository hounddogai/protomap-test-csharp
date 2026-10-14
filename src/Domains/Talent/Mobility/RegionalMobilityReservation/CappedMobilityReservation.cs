namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped mobility reservation is in its life cycle.</summary>
public enum CappedMobilityReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped mobility reservation as the mobility team keeps it.</summary>
public sealed class CappedMobilityReservation(string code, string name, int expiresAt, string externalRef, double fte, string locale, int quantity, string notes, CappedMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public string Locale { get; set; } = locale;
    public int Quantity { get; set; } = quantity;
    public string Notes { get; set; } = notes;
    public CappedMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped mobility reservation in status open.</summary>
    public static CappedMobilityReservation NewCappedMobilityReservation(string code, string name)
    {
        return new CappedMobilityReservation(
            code,
            name,
            0,
            "",
            0.0,
            "",
            0,
            "",
            CappedMobilityReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMobilityReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMobilityReservationStatus next)
    {
        switch (Status)
        {
            case CappedMobilityReservationStatus.Open:
                switch (next)
                {
                    case CappedMobilityReservationStatus.Archived:
                        return true;
                }
                return false;
            case CappedMobilityReservationStatus.InReview:
                switch (next)
                {
                    case CappedMobilityReservationStatus.Reopened:
                    case CappedMobilityReservationStatus.Archived:
                        return true;
                }
                return false;
            case CappedMobilityReservationStatus.Resolved:
                return next == CappedMobilityReservationStatus.Archived;
            case CappedMobilityReservationStatus.Reopened:
                return next == CappedMobilityReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Quantity * (double)ExpiresAt, 2);
    }

    public CappedMobilityReservation Copy()
    {
        return new CappedMobilityReservation(
            Code,
            Name,
            ExpiresAt,
            ExternalRef,
            Fte,
            Locale,
            Quantity,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
