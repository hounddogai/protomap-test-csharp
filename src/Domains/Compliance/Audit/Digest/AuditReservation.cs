namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit reservation is in its life cycle.</summary>
public enum AuditReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One audit reservation, with what the planners need to act on it.</summary>
public sealed class AuditReservation(string code, string title, string managerId, string badge, string timezone, double budget, double fte, double margin, int startsAt, AuditReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public string Timezone { get; set; } = timezone;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public int StartsAt { get; set; } = startsAt;
    public AuditReservationStatus Status { get; set; } = status;

    /// <summary>Starts a audit reservation in status requested.</summary>
    public static AuditReservation NewAuditReservation(string code, string title)
    {
        return new AuditReservation(code, title, "", "", "", 0.0, 0.0, 0.0, 0, AuditReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditReservationStatus next)
    {
        switch (Status)
        {
            case AuditReservationStatus.Requested:
                switch (next)
                {
                    case AuditReservationStatus.Expired:
                        return true;
                }
                return false;
            case AuditReservationStatus.Granted:
                switch (next)
                {
                    case AuditReservationStatus.Revoked:
                    case AuditReservationStatus.Expired:
                        return true;
                }
                return false;
            case AuditReservationStatus.Revoked:
                switch (next)
                {
                    case AuditReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round(Budget * Fte, 2);
    }

    public AuditReservation Copy()
    {
        return new AuditReservation(Code, Title, ManagerId, Badge, Timezone, Budget, Fte, Margin, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
