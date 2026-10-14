namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled statement of work record is in its life cycle.</summary>
public enum ScheduledStatementOfWorkRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled statement of work record. Its code is unique within a tenant.</summary>
public sealed class ScheduledStatementOfWorkRecord(string code, string name, double margin, string badge, int leaveDays, int version, double minimum, string summary, string currency, ScheduledStatementOfWorkRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public int LeaveDays { get; set; } = leaveDays;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public ScheduledStatementOfWorkRecordStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled statement of work record in status open.</summary>
    public static ScheduledStatementOfWorkRecord NewScheduledStatementOfWorkRecord(string code, string name)
    {
        return new ScheduledStatementOfWorkRecord(
            code,
            name,
            0.0,
            "",
            0,
            0,
            0.0,
            "",
            "",
            ScheduledStatementOfWorkRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledStatementOfWorkRecordStatus next)
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkRecordStatus.Open:
                return next == ScheduledStatementOfWorkRecordStatus.Reopened;
            case ScheduledStatementOfWorkRecordStatus.InReview:
                switch (next)
                {
                    case ScheduledStatementOfWorkRecordStatus.Reopened:
                        return true;
                }
                return false;
            case ScheduledStatementOfWorkRecordStatus.Resolved:
                return next == ScheduledStatementOfWorkRecordStatus.Reopened || next == ScheduledStatementOfWorkRecordStatus.Archived;
            case ScheduledStatementOfWorkRecordStatus.Reopened:
                return next == ScheduledStatementOfWorkRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledStatementOfWorkRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled statement of work record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)LeaveDays + Margin, 2);
    }

    public ScheduledStatementOfWorkRecord Copy()
    {
        return new ScheduledStatementOfWorkRecord(
            Code,
            Name,
            Margin,
            Badge,
            LeaveDays,
            Version,
            Minimum,
            Summary,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
