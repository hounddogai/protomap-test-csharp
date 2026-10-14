namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global timesheet batch is in its life cycle.</summary>
public enum GlobalTimesheetBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global timesheet batch as the timesheet team keeps it.</summary>
public sealed class GlobalTimesheetBatch(string code, string title, string phone, double rate, string name, string grade, GlobalTimesheetBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public GlobalTimesheetBatchStatus Status { get; set; } = status;

    /// <summary>Starts a global timesheet batch in status draft.</summary>
    public static GlobalTimesheetBatch NewGlobalTimesheetBatch(string code, string title)
    {
        return new GlobalTimesheetBatch(code, title, "", 0.0, "", "", GlobalTimesheetBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalTimesheetBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalTimesheetBatchStatus next)
    {
        switch (Status)
        {
            case GlobalTimesheetBatchStatus.Draft:
                switch (next)
                {
                    case GlobalTimesheetBatchStatus.Pending:
                        return true;
                }
                return false;
            case GlobalTimesheetBatchStatus.Pending:
                switch (next)
                {
                    case GlobalTimesheetBatchStatus.Suspended:
                    case GlobalTimesheetBatchStatus.Closed:
                        return true;
                }
                return false;
            case GlobalTimesheetBatchStatus.Active:
                switch (next)
                {
                    case GlobalTimesheetBatchStatus.Suspended:
                        return true;
                }
                return false;
            case GlobalTimesheetBatchStatus.Suspended:
                return next == GlobalTimesheetBatchStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalTimesheetBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global timesheet batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public GlobalTimesheetBatch Copy()
    {
        return new GlobalTimesheetBatch(Code, Title, Phone, Rate, Name, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
