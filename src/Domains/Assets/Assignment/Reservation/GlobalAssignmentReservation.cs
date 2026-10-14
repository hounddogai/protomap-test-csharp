namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global assignment reservation is in its life cycle.</summary>
public enum GlobalAssignmentReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a global assignment reservation and its current status.</summary>
public sealed class GlobalAssignmentReservation(string code, string title, bool archived, int attempts, string contactEmail, int version, double score, int endsAt, string department, GlobalAssignmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int Attempts { get; set; } = attempts;
    public string ContactEmail { get; set; } = contactEmail;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public GlobalAssignmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a global assignment reservation in status planned.</summary>
    public static GlobalAssignmentReservation NewGlobalAssignmentReservation(string code, string title)
    {
        return new GlobalAssignmentReservation(
            code,
            title,
            false,
            0,
            "",
            0,
            0.0,
            0,
            "",
            GlobalAssignmentReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAssignmentReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAssignmentReservationStatus next)
    {
        switch (Status)
        {
            case GlobalAssignmentReservationStatus.Planned:
                switch (next)
                {
                    case GlobalAssignmentReservationStatus.InProgress:
                        return true;
                }
                return false;
            case GlobalAssignmentReservationStatus.Scheduled:
                switch (next)
                {
                    case GlobalAssignmentReservationStatus.Completed:
                    case GlobalAssignmentReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalAssignmentReservationStatus.InProgress:
                return next == GlobalAssignmentReservationStatus.Completed;
            case GlobalAssignmentReservationStatus.Completed:
                return next == GlobalAssignmentReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalAssignmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global assignment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"global assignment reservation"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts * (double)Version, 2);
    }

    public GlobalAssignmentReservation Copy()
    {
        return new GlobalAssignmentReservation(
            Code,
            Title,
            Archived,
            Attempts,
            ContactEmail,
            Version,
            Score,
            EndsAt,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
