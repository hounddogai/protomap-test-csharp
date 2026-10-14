namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work reservation is in its life cycle.</summary>
public enum StatementOfWorkReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a statement of work reservation and its current status.</summary>
public sealed class StatementOfWorkReservation(string code, string title, int retries, double threshold, string jobCode, string managerId, int updatedAt, string grade, string skill, string team, StatementOfWorkReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public string ManagerId { get; set; } = managerId;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Grade { get; set; } = grade;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public StatementOfWorkReservationStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work reservation in status requested.</summary>
    public static StatementOfWorkReservation NewStatementOfWorkReservation(string code, string title)
    {
        return new StatementOfWorkReservation(
            code,
            title,
            0,
            0.0,
            "",
            "",
            0,
            "",
            "",
            "",
            StatementOfWorkReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkReservationStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkReservationStatus.Requested:
                switch (next)
                {
                    case StatementOfWorkReservationStatus.Granted:
                        return true;
                }
                return false;
            case StatementOfWorkReservationStatus.Granted:
                switch (next)
                {
                    case StatementOfWorkReservationStatus.Revoked:
                    case StatementOfWorkReservationStatus.Expired:
                        return true;
                }
                return false;
            case StatementOfWorkReservationStatus.Revoked:
                switch (next)
                {
                    case StatementOfWorkReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries * Threshold, 2);
    }

    public StatementOfWorkReservation Copy()
    {
        return new StatementOfWorkReservation(
            Code,
            Title,
            Retries,
            Threshold,
            JobCode,
            ManagerId,
            UpdatedAt,
            Grade,
            Skill,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
