namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual refresh reservation is in its life cycle.</summary>
public enum ManualRefreshReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One manual refresh reservation, with what the planners need to act on it.</summary>
public sealed class ManualRefreshReservation(string code, string name, string phone, bool archived, string source, int durationMinutes, double maximum, string region, int expiresAt, double minimum, int priority, ManualRefreshReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Minimum { get; set; } = minimum;
    public int Priority { get; set; } = priority;
    public ManualRefreshReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual refresh reservation in status created.</summary>
    public static ManualRefreshReservation NewManualRefreshReservation(string code, string name)
    {
        return new ManualRefreshReservation(
            code,
            name,
            "",
            false,
            "",
            0,
            0.0,
            "",
            0,
            0.0,
            0,
            ManualRefreshReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRefreshReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRefreshReservationStatus next)
    {
        switch (Status)
        {
            case ManualRefreshReservationStatus.Created:
                return next == ManualRefreshReservationStatus.Retired;
            case ManualRefreshReservationStatus.Validated:
                return next == ManualRefreshReservationStatus.Published || next == ManualRefreshReservationStatus.Retired;
            case ManualRefreshReservationStatus.Published:
                switch (next)
                {
                    case ManualRefreshReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualRefreshReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual refresh reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority + Maximum, 2);
    }

    public ManualRefreshReservation Copy()
    {
        return new ManualRefreshReservation(
            Code,
            Name,
            Phone,
            Archived,
            Source,
            DurationMinutes,
            Maximum,
            Region,
            ExpiresAt,
            Minimum,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
