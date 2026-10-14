namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global timesheet review is in its life cycle.</summary>
public enum GlobalTimesheetReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global timesheet review as the timesheet team keeps it.</summary>
public sealed class GlobalTimesheetReview(string code, string label, string jobCode, double ratio, double weight, int quantity, int startsAt, string title, GlobalTimesheetReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Ratio { get; set; } = ratio;
    public double Weight { get; set; } = weight;
    public int Quantity { get; set; } = quantity;
    public int StartsAt { get; set; } = startsAt;
    public string Title { get; set; } = title;
    public GlobalTimesheetReviewStatus Status { get; set; } = status;

    /// <summary>Starts a global timesheet review in status proposed.</summary>
    public static GlobalTimesheetReview NewGlobalTimesheetReview(string code, string label)
    {
        return new GlobalTimesheetReview(code, label, "", 0.0, 0.0, 0, 0, "", GlobalTimesheetReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalTimesheetReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalTimesheetReviewStatus next)
    {
        switch (Status)
        {
            case GlobalTimesheetReviewStatus.Proposed:
                switch (next)
                {
                    case GlobalTimesheetReviewStatus.Approved:
                    case GlobalTimesheetReviewStatus.Rejected:
                        return true;
                }
                return false;
            case GlobalTimesheetReviewStatus.Approved:
                return next == GlobalTimesheetReviewStatus.Rejected || next == GlobalTimesheetReviewStatus.Withdrawn;
            case GlobalTimesheetReviewStatus.Rejected:
                return next == GlobalTimesheetReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalTimesheetReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global timesheet review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global timesheet review"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)StartsAt - Ratio, 2);
    }

    public GlobalTimesheetReview Copy()
    {
        return new GlobalTimesheetReview(Code, Label, JobCode, Ratio, Weight, Quantity, StartsAt, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
