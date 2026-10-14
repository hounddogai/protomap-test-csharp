namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted engagement reservation is in its life cycle.</summary>
public enum WeightedEngagementReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted engagement reservation, with what the planners need to act on it.</summary>
public sealed class WeightedEngagementReservation(string code, string label, bool approved, double minimum, double rating, string country, bool active, string contactEmail, int sequence, int leaveDays, WeightedEngagementReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Minimum { get; set; } = minimum;
    public double Rating { get; set; } = rating;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public string ContactEmail { get; set; } = contactEmail;
    public int Sequence { get; set; } = sequence;
    public int LeaveDays { get; set; } = leaveDays;
    public WeightedEngagementReservationStatus Status { get; set; } = status;

    /// <summary>Starts a weighted engagement reservation in status open.</summary>
    public static WeightedEngagementReservation NewWeightedEngagementReservation(string code, string label)
    {
        return new WeightedEngagementReservation(
            code,
            label,
            false,
            0.0,
            0.0,
            "",
            false,
            "",
            0,
            0,
            WeightedEngagementReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEngagementReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEngagementReservationStatus next)
    {
        switch (Status)
        {
            case WeightedEngagementReservationStatus.Open:
                return next == WeightedEngagementReservationStatus.Archived;
            case WeightedEngagementReservationStatus.InReview:
                switch (next)
                {
                    case WeightedEngagementReservationStatus.Resolved:
                    case WeightedEngagementReservationStatus.Archived:
                        return true;
                }
                return false;
            case WeightedEngagementReservationStatus.Resolved:
                return next == WeightedEngagementReservationStatus.Reopened || next == WeightedEngagementReservationStatus.Archived;
            case WeightedEngagementReservationStatus.Reopened:
                switch (next)
                {
                    case WeightedEngagementReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedEngagementReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted engagement reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted engagement reservation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Minimum - Rating, 2);
    }

    public WeightedEngagementReservation Copy()
    {
        return new WeightedEngagementReservation(
            Code,
            Label,
            Approved,
            Minimum,
            Rating,
            Country,
            Active,
            ContactEmail,
            Sequence,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
