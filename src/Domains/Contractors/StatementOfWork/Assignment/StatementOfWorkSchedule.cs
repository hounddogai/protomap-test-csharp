namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work schedule is in its life cycle.</summary>
public enum StatementOfWorkScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A statement of work schedule as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkSchedule(string code, string name, bool pinned, double minimum, int endsAt, int createdAt, bool approved, StatementOfWorkScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public StatementOfWorkScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work schedule in status draft.</summary>
    public static StatementOfWorkSchedule NewStatementOfWorkSchedule(string code, string name)
    {
        return new StatementOfWorkSchedule(code, name, false, 0.0, 0, 0, false, StatementOfWorkScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkScheduleStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkScheduleStatus.Draft:
                return next == StatementOfWorkScheduleStatus.Closed;
            case StatementOfWorkScheduleStatus.Pending:
                switch (next)
                {
                    case StatementOfWorkScheduleStatus.Suspended:
                        return true;
                }
                return false;
            case StatementOfWorkScheduleStatus.Active:
                switch (next)
                {
                    case StatementOfWorkScheduleStatus.Suspended:
                    case StatementOfWorkScheduleStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkScheduleStatus.Suspended:
                return next == StatementOfWorkScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Minimum * (double)EndsAt, 2);
    }

    public StatementOfWorkSchedule Copy()
    {
        return new StatementOfWorkSchedule(Code, Name, Pinned, Minimum, EndsAt, CreatedAt, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
