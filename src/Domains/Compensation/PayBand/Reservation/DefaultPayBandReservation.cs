namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay band reservation is in its life cycle.</summary>
public enum DefaultPayBandReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a default pay band reservation and its current status.</summary>
public sealed class DefaultPayBandReservation(string code, string title, int createdAt, double fte, int version, int retries, int windowDays, DefaultPayBandReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public int Retries { get; set; } = retries;
    public int WindowDays { get; set; } = windowDays;
    public DefaultPayBandReservationStatus Status { get; set; } = status;

    /// <summary>Starts a default pay band reservation in status requested.</summary>
    public static DefaultPayBandReservation NewDefaultPayBandReservation(string code, string title)
    {
        return new DefaultPayBandReservation(
            code,
            title,
            0,
            0.0,
            0,
            0,
            0,
            DefaultPayBandReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayBandReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayBandReservationStatus next)
    {
        switch (Status)
        {
            case DefaultPayBandReservationStatus.Requested:
                return next == DefaultPayBandReservationStatus.Expired;
            case DefaultPayBandReservationStatus.Granted:
                return next == DefaultPayBandReservationStatus.Revoked || next == DefaultPayBandReservationStatus.Expired;
            case DefaultPayBandReservationStatus.Revoked:
                switch (next)
                {
                    case DefaultPayBandReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayBandReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay band reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"default pay band reservation"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)WindowDays + Fte, 2);
    }

    public DefaultPayBandReservation Copy()
    {
        return new DefaultPayBandReservation(Code, Title, CreatedAt, Fte, Version, Retries, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
