namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work group is in its life cycle.</summary>
public enum StatementOfWorkGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A statement of work group. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkGroup(string code, string label, double minimum, int graceDays, string unit, string name, string grade, string title, bool approved, double score, string externalRef, StatementOfWorkGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int GraceDays { get; set; } = graceDays;
    public string Unit { get; set; } = unit;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public StatementOfWorkGroupStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work group in status draft.</summary>
    public static StatementOfWorkGroup NewStatementOfWorkGroup(string code, string label)
    {
        return new StatementOfWorkGroup(
            code,
            label,
            0.0,
            0,
            "",
            "",
            "",
            "",
            false,
            0.0,
            "",
            StatementOfWorkGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkGroupStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkGroupStatus.Draft:
                return next == StatementOfWorkGroupStatus.Active;
            case StatementOfWorkGroupStatus.Pending:
                return next == StatementOfWorkGroupStatus.Active || next == StatementOfWorkGroupStatus.Suspended;
            case StatementOfWorkGroupStatus.Active:
                switch (next)
                {
                    case StatementOfWorkGroupStatus.Suspended:
                        return true;
                }
                return false;
            case StatementOfWorkGroupStatus.Suspended:
                switch (next)
                {
                    case StatementOfWorkGroupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"statement of work group"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays - Minimum, 2);
    }

    public StatementOfWorkGroup Copy()
    {
        return new StatementOfWorkGroup(
            Code,
            Label,
            Minimum,
            GraceDays,
            Unit,
            Name,
            Grade,
            Title,
            Approved,
            Score,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
