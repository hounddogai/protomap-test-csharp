namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday reservation is in its life cycle.</summary>
public enum HolidayReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a holiday reservation and its current status.</summary>
public sealed class HolidayReservation(string code, string name, double rate, string timezone, string locale, double margin, double score, int createdAt, string location, double maximum, string checksum, HolidayReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public string Locale { get; set; } = locale;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public string Checksum { get; set; } = checksum;
    public HolidayReservationStatus Status { get; set; } = status;

    /// <summary>Starts a holiday reservation in status requested.</summary>
    public static HolidayReservation NewHolidayReservation(string code, string name)
    {
        return new HolidayReservation(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            HolidayReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayReservationStatus next)
    {
        switch (Status)
        {
            case HolidayReservationStatus.Requested:
                return next == HolidayReservationStatus.Expired;
            case HolidayReservationStatus.Granted:
                switch (next)
                {
                    case HolidayReservationStatus.Revoked:
                    case HolidayReservationStatus.Expired:
                        return true;
                }
                return false;
            case HolidayReservationStatus.Revoked:
                switch (next)
                {
                    case HolidayReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"holiday reservation"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Rate * Maximum, 2);
    }

    public HolidayReservation Copy()
    {
        return new HolidayReservation(
            Code,
            Name,
            Rate,
            Timezone,
            Locale,
            Margin,
            Score,
            CreatedAt,
            Location,
            Maximum,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
