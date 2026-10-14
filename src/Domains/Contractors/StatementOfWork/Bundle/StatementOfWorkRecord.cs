namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work record is in its life cycle.</summary>
public enum StatementOfWorkRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A statement of work record as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkRecord(string code, string name, string category, double hours, bool active, string phone, int tenureMonths, string skill, string notes, string grade, int priority, StatementOfWorkRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public StatementOfWorkRecordStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work record in status proposed.</summary>
    public static StatementOfWorkRecord NewStatementOfWorkRecord(string code, string name)
    {
        return new StatementOfWorkRecord(
            code,
            name,
            "",
            0.0,
            false,
            "",
            0,
            "",
            "",
            "",
            0,
            StatementOfWorkRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkRecordStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkRecordStatus.Proposed:
                switch (next)
                {
                    case StatementOfWorkRecordStatus.Rejected:
                        return true;
                }
                return false;
            case StatementOfWorkRecordStatus.Approved:
                return next == StatementOfWorkRecordStatus.Rejected || next == StatementOfWorkRecordStatus.Withdrawn;
            case StatementOfWorkRecordStatus.Rejected:
                return next == StatementOfWorkRecordStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"statement of work record"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority + (double)TenureMonths, 2);
    }

    public StatementOfWorkRecord Copy()
    {
        return new StatementOfWorkRecord(
            Code,
            Name,
            Category,
            Hours,
            Active,
            Phone,
            TenureMonths,
            Skill,
            Notes,
            Grade,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
