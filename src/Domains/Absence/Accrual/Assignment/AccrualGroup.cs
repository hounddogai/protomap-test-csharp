namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual group is in its life cycle.</summary>
public enum AccrualGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a accrual group and its current status.</summary>
public sealed class AccrualGroup(string code, string name, int revision, string source, int batchSize, double ceiling, string employeeId, string grade, int priority, AccrualGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public double Ceiling { get; set; } = ceiling;
    public string EmployeeId { get; set; } = employeeId;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public AccrualGroupStatus Status { get; set; } = status;

    /// <summary>Starts a accrual group in status planned.</summary>
    public static AccrualGroup NewAccrualGroup(string code, string name)
    {
        return new AccrualGroup(code, name, 0, "", 0, 0.0, "", "", 0, AccrualGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualGroupStatus next)
    {
        switch (Status)
        {
            case AccrualGroupStatus.Planned:
                return next == AccrualGroupStatus.Cancelled;
            case AccrualGroupStatus.Scheduled:
                return next == AccrualGroupStatus.Completed || next == AccrualGroupStatus.Cancelled;
            case AccrualGroupStatus.InProgress:
                switch (next)
                {
                    case AccrualGroupStatus.Completed:
                    case AccrualGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualGroupStatus.Completed:
                switch (next)
                {
                    case AccrualGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling - (double)Revision, 2);
    }

    public AccrualGroup Copy()
    {
        return new AccrualGroup(
            Code,
            Name,
            Revision,
            Source,
            BatchSize,
            Ceiling,
            EmployeeId,
            Grade,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
