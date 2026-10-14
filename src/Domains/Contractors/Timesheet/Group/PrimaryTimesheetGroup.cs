namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary timesheet group is in its life cycle.</summary>
public enum PrimaryTimesheetGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary timesheet group and its current status.</summary>
public sealed class PrimaryTimesheetGroup(string code, string name, string ownerId, string jobCode, int graceDays, double weight, bool active, string country, PrimaryTimesheetGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public PrimaryTimesheetGroupStatus Status { get; set; } = status;

    /// <summary>Starts a primary timesheet group in status draft.</summary>
    public static PrimaryTimesheetGroup NewPrimaryTimesheetGroup(string code, string name)
    {
        return new PrimaryTimesheetGroup(code, name, "", "", 0, 0.0, false, "", PrimaryTimesheetGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTimesheetGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTimesheetGroupStatus next)
    {
        switch (Status)
        {
            case PrimaryTimesheetGroupStatus.Draft:
                switch (next)
                {
                    case PrimaryTimesheetGroupStatus.Active:
                    case PrimaryTimesheetGroupStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryTimesheetGroupStatus.Pending:
                return next == PrimaryTimesheetGroupStatus.Suspended;
            case PrimaryTimesheetGroupStatus.Active:
                switch (next)
                {
                    case PrimaryTimesheetGroupStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryTimesheetGroupStatus.Suspended:
                switch (next)
                {
                    case PrimaryTimesheetGroupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryTimesheetGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary timesheet group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)GraceDays - Weight, 2);
    }

    public PrimaryTimesheetGroup Copy()
    {
        return new PrimaryTimesheetGroup(Code, Name, OwnerId, JobCode, GraceDays, Weight, Active, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
