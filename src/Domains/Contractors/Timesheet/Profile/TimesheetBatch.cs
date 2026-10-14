namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet batch is in its life cycle.</summary>
public enum TimesheetBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One timesheet batch, with what the planners need to act on it.</summary>
public sealed class TimesheetBatch(string code, string name, double cost, bool approved, double threshold, int capacity, TimesheetBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public TimesheetBatchStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet batch in status created.</summary>
    public static TimesheetBatch NewTimesheetBatch(string code, string name)
    {
        return new TimesheetBatch(code, name, 0.0, false, 0.0, 0, TimesheetBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetBatchStatus next)
    {
        switch (Status)
        {
            case TimesheetBatchStatus.Created:
                switch (next)
                {
                    case TimesheetBatchStatus.Validated:
                        return true;
                }
                return false;
            case TimesheetBatchStatus.Validated:
                switch (next)
                {
                    case TimesheetBatchStatus.Published:
                        return true;
                }
                return false;
            case TimesheetBatchStatus.Published:
                switch (next)
                {
                    case TimesheetBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold * Cost, 2);
    }

    public TimesheetBatch Copy()
    {
        return new TimesheetBatch(Code, Name, Cost, Approved, Threshold, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
