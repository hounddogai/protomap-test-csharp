namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending timesheet summary is in its life cycle.</summary>
public enum PendingTimesheetSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending timesheet summary as the timesheet team keeps it.</summary>
public sealed class PendingTimesheetSummary(string code, string title, string checksum, string badge, string category, string reviewer, string phone, double amount, PendingTimesheetSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public PendingTimesheetSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a pending timesheet summary in status idle.</summary>
    public static PendingTimesheetSummary NewPendingTimesheetSummary(string code, string title)
    {
        return new PendingTimesheetSummary(code, title, "", "", "", "", "", 0.0, PendingTimesheetSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTimesheetSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTimesheetSummaryStatus next)
    {
        switch (Status)
        {
            case PendingTimesheetSummaryStatus.Idle:
                switch (next)
                {
                    case PendingTimesheetSummaryStatus.Draining:
                    case PendingTimesheetSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case PendingTimesheetSummaryStatus.Warming:
                return next == PendingTimesheetSummaryStatus.Draining;
            case PendingTimesheetSummaryStatus.Ready:
                return next == PendingTimesheetSummaryStatus.Draining || next == PendingTimesheetSummaryStatus.Stopped;
            case PendingTimesheetSummaryStatus.Draining:
                switch (next)
                {
                    case PendingTimesheetSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTimesheetSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending timesheet summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending timesheet summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PendingTimesheetSummary Copy()
    {
        return new PendingTimesheetSummary(Code, Title, Checksum, Badge, Category, Reviewer, Phone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
