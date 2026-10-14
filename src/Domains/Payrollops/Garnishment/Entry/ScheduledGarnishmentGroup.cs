namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled garnishment group is in its life cycle.</summary>
public enum ScheduledGarnishmentGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled garnishment group and its current status.</summary>
public sealed class ScheduledGarnishmentGroup(string code, string name, double hours, double maximum, int priority, string description, double rating, ScheduledGarnishmentGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public int Priority { get; set; } = priority;
    public string Description { get; set; } = description;
    public double Rating { get; set; } = rating;
    public ScheduledGarnishmentGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled garnishment group in status created.</summary>
    public static ScheduledGarnishmentGroup NewScheduledGarnishmentGroup(string code, string name)
    {
        return new ScheduledGarnishmentGroup(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            0.0,
            ScheduledGarnishmentGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledGarnishmentGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledGarnishmentGroupStatus next)
    {
        switch (Status)
        {
            case ScheduledGarnishmentGroupStatus.Created:
                return next == ScheduledGarnishmentGroupStatus.Validated;
            case ScheduledGarnishmentGroupStatus.Validated:
                return next == ScheduledGarnishmentGroupStatus.Published || next == ScheduledGarnishmentGroupStatus.Retired;
            case ScheduledGarnishmentGroupStatus.Published:
                return next == ScheduledGarnishmentGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledGarnishmentGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled garnishment group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round(Rating * Maximum, 2);
    }

    public ScheduledGarnishmentGroup Copy()
    {
        return new ScheduledGarnishmentGroup(Code, Name, Hours, Maximum, Priority, Description, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
