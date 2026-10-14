namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement reservation is in its life cycle.</summary>
public enum EngagementReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a engagement reservation and its current status.</summary>
public sealed class EngagementReservation(string code, string title, int startsAt, int tenureMonths, string location, string phone, double ratio, EngagementReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public double Ratio { get; set; } = ratio;
    public EngagementReservationStatus Status { get; set; } = status;

    /// <summary>Starts a engagement reservation in status requested.</summary>
    public static EngagementReservation NewEngagementReservation(string code, string title)
    {
        return new EngagementReservation(code, title, 0, 0, "", "", 0.0, EngagementReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementReservationStatus next)
    {
        switch (Status)
        {
            case EngagementReservationStatus.Requested:
                switch (next)
                {
                    case EngagementReservationStatus.Revoked:
                        return true;
                }
                return false;
            case EngagementReservationStatus.Granted:
                switch (next)
                {
                    case EngagementReservationStatus.Expired:
                        return true;
                }
                return false;
            case EngagementReservationStatus.Revoked:
                switch (next)
                {
                    case EngagementReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"engagement reservation"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio * (double)StartsAt, 2);
    }

    public EngagementReservation Copy()
    {
        return new EngagementReservation(Code, Title, StartsAt, TenureMonths, Location, Phone, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
