namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy nomination reservation is in its life cycle.</summary>
public enum LegacyNominationReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy nomination reservation. Its code is unique within a tenant.</summary>
public sealed class LegacyNominationReservation(string code, string label, double rating, string summary, double score, string description, LegacyNominationReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public string Description { get; set; } = description;
    public LegacyNominationReservationStatus Status { get; set; } = status;

    /// <summary>Starts a legacy nomination reservation in status idle.</summary>
    public static LegacyNominationReservation NewLegacyNominationReservation(string code, string label)
    {
        return new LegacyNominationReservation(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            LegacyNominationReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyNominationReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyNominationReservationStatus next)
    {
        switch (Status)
        {
            case LegacyNominationReservationStatus.Idle:
                return next == LegacyNominationReservationStatus.Ready || next == LegacyNominationReservationStatus.Draining;
            case LegacyNominationReservationStatus.Warming:
                return next == LegacyNominationReservationStatus.Ready || next == LegacyNominationReservationStatus.Stopped;
            case LegacyNominationReservationStatus.Ready:
                switch (next)
                {
                    case LegacyNominationReservationStatus.Draining:
                        return true;
                }
                return false;
            case LegacyNominationReservationStatus.Draining:
                switch (next)
                {
                    case LegacyNominationReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyNominationReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy nomination reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rating - Score, 2);
    }

    public LegacyNominationReservation Copy()
    {
        return new LegacyNominationReservation(Code, Label, Rating, Summary, Score, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
