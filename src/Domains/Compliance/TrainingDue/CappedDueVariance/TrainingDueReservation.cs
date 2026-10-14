namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due reservation is in its life cycle.</summary>
public enum TrainingDueReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One training due reservation, with what the planners need to act on it.</summary>
public sealed class TrainingDueReservation(string code, string label, string employeeId, bool pinned, string salaryBand, string team, double amount, TrainingDueReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public bool Pinned { get; set; } = pinned;
    public string SalaryBand { get; set; } = salaryBand;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public TrainingDueReservationStatus Status { get; set; } = status;

    /// <summary>Starts a training due reservation in status requested.</summary>
    public static TrainingDueReservation NewTrainingDueReservation(string code, string label)
    {
        return new TrainingDueReservation(
            code,
            label,
            "",
            false,
            "",
            "",
            0.0,
            TrainingDueReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueReservationStatus next)
    {
        switch (Status)
        {
            case TrainingDueReservationStatus.Requested:
                switch (next)
                {
                    case TrainingDueReservationStatus.Expired:
                        return true;
                }
                return false;
            case TrainingDueReservationStatus.Granted:
                return next == TrainingDueReservationStatus.Expired;
            case TrainingDueReservationStatus.Revoked:
                return next == TrainingDueReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TrainingDueReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public TrainingDueReservation Copy()
    {
        return new TrainingDueReservation(Code, Label, EmployeeId, Pinned, SalaryBand, Team, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
