namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional timesheet review is in its life cycle.</summary>
public enum RegionalTimesheetReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional timesheet review and its current status.</summary>
public sealed class RegionalTimesheetReview(string code, string label, string team, string badge, string source, string location, int attempts, string description, RegionalTimesheetReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Badge { get; set; } = badge;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public RegionalTimesheetReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional timesheet review in status created.</summary>
    public static RegionalTimesheetReview NewRegionalTimesheetReview(string code, string label)
    {
        return new RegionalTimesheetReview(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            "",
            RegionalTimesheetReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTimesheetReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTimesheetReviewStatus next)
    {
        switch (Status)
        {
            case RegionalTimesheetReviewStatus.Created:
                return next == RegionalTimesheetReviewStatus.Published;
            case RegionalTimesheetReviewStatus.Validated:
                return next == RegionalTimesheetReviewStatus.Published;
            case RegionalTimesheetReviewStatus.Published:
                return next == RegionalTimesheetReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalTimesheetReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional timesheet review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public RegionalTimesheetReview Copy()
    {
        return new RegionalTimesheetReview(
            Code,
            Label,
            Team,
            Badge,
            Source,
            Location,
            Attempts,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
