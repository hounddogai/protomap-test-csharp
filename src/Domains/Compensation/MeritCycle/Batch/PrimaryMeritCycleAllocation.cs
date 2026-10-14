namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary merit cycle allocation is in its life cycle.</summary>
public enum PrimaryMeritCycleAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary merit cycle allocation as the merit cycle team keeps it.</summary>
public sealed class PrimaryMeritCycleAllocation(string code, string title, bool active, int leaveDays, string notes, PrimaryMeritCycleAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public string Notes { get; set; } = notes;
    public PrimaryMeritCycleAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a primary merit cycle allocation in status planned.</summary>
    public static PrimaryMeritCycleAllocation NewPrimaryMeritCycleAllocation(string code, string title)
    {
        return new PrimaryMeritCycleAllocation(code, title, false, 0, "", PrimaryMeritCycleAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMeritCycleAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMeritCycleAllocationStatus next)
    {
        switch (Status)
        {
            case PrimaryMeritCycleAllocationStatus.Planned:
                switch (next)
                {
                    case PrimaryMeritCycleAllocationStatus.InProgress:
                    case PrimaryMeritCycleAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryMeritCycleAllocationStatus.Scheduled:
                switch (next)
                {
                    case PrimaryMeritCycleAllocationStatus.Completed:
                    case PrimaryMeritCycleAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryMeritCycleAllocationStatus.InProgress:
                switch (next)
                {
                    case PrimaryMeritCycleAllocationStatus.Completed:
                    case PrimaryMeritCycleAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryMeritCycleAllocationStatus.Completed:
                switch (next)
                {
                    case PrimaryMeritCycleAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryMeritCycleAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary merit cycle allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PrimaryMeritCycleAllocation Copy()
    {
        return new PrimaryMeritCycleAllocation(Code, Title, Active, LeaveDays, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
