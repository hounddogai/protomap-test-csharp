namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work segment is in its life cycle.</summary>
public enum StatementOfWorkSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One statement of work segment, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkSegment(string code, string name, bool approved, int quantity, string grade, bool active, double maximum, StatementOfWorkSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public double Maximum { get; set; } = maximum;
    public StatementOfWorkSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work segment in status planned.</summary>
    public static StatementOfWorkSegment NewStatementOfWorkSegment(string code, string name)
    {
        return new StatementOfWorkSegment(
            code,
            name,
            false,
            0,
            "",
            false,
            0.0,
            StatementOfWorkSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkSegmentStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkSegmentStatus.Planned:
                return next == StatementOfWorkSegmentStatus.Cancelled;
            case StatementOfWorkSegmentStatus.Scheduled:
                switch (next)
                {
                    case StatementOfWorkSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case StatementOfWorkSegmentStatus.InProgress:
                return next == StatementOfWorkSegmentStatus.Cancelled;
            case StatementOfWorkSegmentStatus.Completed:
                return next == StatementOfWorkSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Maximum * (double)Quantity, 2);
    }

    public StatementOfWorkSegment Copy()
    {
        return new StatementOfWorkSegment(Code, Name, Approved, Quantity, Grade, Active, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
