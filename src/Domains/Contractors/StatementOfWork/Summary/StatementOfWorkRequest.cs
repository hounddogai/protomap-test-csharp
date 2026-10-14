namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work request is in its life cycle.</summary>
public enum StatementOfWorkRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a statement of work request and its current status.</summary>
public sealed class StatementOfWorkRequest(string code, string title, string salaryBand, string team, string timezone, int attempts, string channel, string locale, StatementOfWorkRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public int Attempts { get; set; } = attempts;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public StatementOfWorkRequestStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work request in status planned.</summary>
    public static StatementOfWorkRequest NewStatementOfWorkRequest(string code, string title)
    {
        return new StatementOfWorkRequest(code, title, "", "", "", 0, "", "", StatementOfWorkRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkRequestStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkRequestStatus.Planned:
                switch (next)
                {
                    case StatementOfWorkRequestStatus.InProgress:
                    case StatementOfWorkRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case StatementOfWorkRequestStatus.Scheduled:
                return next == StatementOfWorkRequestStatus.Cancelled;
            case StatementOfWorkRequestStatus.InProgress:
                switch (next)
                {
                    case StatementOfWorkRequestStatus.Completed:
                    case StatementOfWorkRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case StatementOfWorkRequestStatus.Completed:
                return next == StatementOfWorkRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public StatementOfWorkRequest Copy()
    {
        return new StatementOfWorkRequest(
            Code,
            Title,
            SalaryBand,
            Team,
            Timezone,
            Attempts,
            Channel,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
