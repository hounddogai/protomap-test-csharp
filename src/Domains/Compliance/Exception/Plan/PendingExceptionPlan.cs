namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception plan is in its life cycle.</summary>
public enum PendingExceptionPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending exception plan as the exception team keeps it.</summary>
public sealed class PendingExceptionPlan(string code, string title, int batchSize, string reviewer, bool approved, string segment, PendingExceptionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public bool Approved { get; set; } = approved;
    public string Segment { get; set; } = segment;
    public PendingExceptionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception plan in status planned.</summary>
    public static PendingExceptionPlan NewPendingExceptionPlan(string code, string title)
    {
        return new PendingExceptionPlan(code, title, 0, "", false, "", PendingExceptionPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionPlanStatus next)
    {
        switch (Status)
        {
            case PendingExceptionPlanStatus.Planned:
                return next == PendingExceptionPlanStatus.Scheduled;
            case PendingExceptionPlanStatus.Scheduled:
                switch (next)
                {
                    case PendingExceptionPlanStatus.InProgress:
                        return true;
                }
                return false;
            case PendingExceptionPlanStatus.InProgress:
                return next == PendingExceptionPlanStatus.Completed;
            case PendingExceptionPlanStatus.Completed:
                switch (next)
                {
                    case PendingExceptionPlanStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingExceptionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending exception plan"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public PendingExceptionPlan Copy()
    {
        return new PendingExceptionPlan(Code, Title, BatchSize, Reviewer, Approved, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
