namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary timesheet adjustment is in its life cycle.</summary>
public enum PrimaryTimesheetAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary timesheet adjustment and its current status.</summary>
public sealed class PrimaryTimesheetAdjustment(string code, string title, bool archived, string timezone, int durationMinutes, double cost, string salaryBand, string department, string channel, int limit, string unit, PrimaryTimesheetAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public string Department { get; set; } = department;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public PrimaryTimesheetAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary timesheet adjustment in status draft.</summary>
    public static PrimaryTimesheetAdjustment NewPrimaryTimesheetAdjustment(string code, string title)
    {
        return new PrimaryTimesheetAdjustment(
            code,
            title,
            false,
            "",
            0,
            0.0,
            "",
            "",
            "",
            0,
            "",
            PrimaryTimesheetAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTimesheetAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTimesheetAdjustmentStatus next)
    {
        switch (Status)
        {
            case PrimaryTimesheetAdjustmentStatus.Draft:
                switch (next)
                {
                    case PrimaryTimesheetAdjustmentStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryTimesheetAdjustmentStatus.Pending:
                return next == PrimaryTimesheetAdjustmentStatus.Active;
            case PrimaryTimesheetAdjustmentStatus.Active:
                switch (next)
                {
                    case PrimaryTimesheetAdjustmentStatus.Suspended:
                    case PrimaryTimesheetAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryTimesheetAdjustmentStatus.Suspended:
                return next == PrimaryTimesheetAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryTimesheetAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary timesheet adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round(Cost * (double)Limit, 2);
    }

    public PrimaryTimesheetAdjustment Copy()
    {
        return new PrimaryTimesheetAdjustment(
            Code,
            Title,
            Archived,
            Timezone,
            DurationMinutes,
            Cost,
            SalaryBand,
            Department,
            Channel,
            Limit,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
