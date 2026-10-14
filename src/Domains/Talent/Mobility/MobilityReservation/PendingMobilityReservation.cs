namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending mobility reservation is in its life cycle.</summary>
public enum PendingMobilityReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending mobility reservation, with what the planners need to act on it.</summary>
public sealed class PendingMobilityReservation(string code, string title, string externalRef, string skill, string notes, string department, double threshold, bool visible, List<string> tags, PendingMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Skill { get; set; } = skill;
    public string Notes { get; set; } = notes;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public PendingMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pending mobility reservation in status requested.</summary>
    public static PendingMobilityReservation NewPendingMobilityReservation(string code, string title)
    {
        return new PendingMobilityReservation(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            false,
            new List<string>(),
            PendingMobilityReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingMobilityReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingMobilityReservationStatus next)
    {
        switch (Status)
        {
            case PendingMobilityReservationStatus.Requested:
                switch (next)
                {
                    case PendingMobilityReservationStatus.Revoked:
                        return true;
                }
                return false;
            case PendingMobilityReservationStatus.Granted:
                switch (next)
                {
                    case PendingMobilityReservationStatus.Expired:
                        return true;
                }
                return false;
            case PendingMobilityReservationStatus.Revoked:
                return next == PendingMobilityReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public PendingMobilityReservation Copy()
    {
        return new PendingMobilityReservation(
            Code,
            Title,
            ExternalRef,
            Skill,
            Notes,
            Department,
            Threshold,
            Visible,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
