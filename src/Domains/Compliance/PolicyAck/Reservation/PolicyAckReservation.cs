namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack reservation is in its life cycle.</summary>
public enum PolicyAckReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One policy ack reservation, with what the planners need to act on it.</summary>
public sealed class PolicyAckReservation(string code, string title, string managerId, string timezone, double hours, double cost, double rating, int expiresAt, string reviewer, int endsAt, Dictionary<string, string> attributes, PolicyAckReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public double Cost { get; set; } = cost;
    public double Rating { get; set; } = rating;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PolicyAckReservationStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack reservation in status open.</summary>
    public static PolicyAckReservation NewPolicyAckReservation(string code, string title)
    {
        return new PolicyAckReservation(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0.0,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            PolicyAckReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckReservationStatus next)
    {
        switch (Status)
        {
            case PolicyAckReservationStatus.Open:
                switch (next)
                {
                    case PolicyAckReservationStatus.Reopened:
                    case PolicyAckReservationStatus.Archived:
                        return true;
                }
                return false;
            case PolicyAckReservationStatus.InReview:
                return next == PolicyAckReservationStatus.Reopened || next == PolicyAckReservationStatus.Archived;
            case PolicyAckReservationStatus.Resolved:
                switch (next)
                {
                    case PolicyAckReservationStatus.Reopened:
                    case PolicyAckReservationStatus.Archived:
                        return true;
                }
                return false;
            case PolicyAckReservationStatus.Reopened:
                return next == PolicyAckReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PolicyAckReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round(Hours + Cost, 2);
    }

    public PolicyAckReservation Copy()
    {
        return new PolicyAckReservation(
            Code,
            Title,
            ManagerId,
            Timezone,
            Hours,
            Cost,
            Rating,
            ExpiresAt,
            Reviewer,
            EndsAt,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
