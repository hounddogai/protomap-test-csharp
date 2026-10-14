namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary timesheet plan is in its life cycle.</summary>
public enum PrimaryTimesheetPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary timesheet plan as the timesheet team keeps it.</summary>
public sealed class PrimaryTimesheetPlan(string code, string name, double amount, string timezone, double margin, string grade, Dictionary<string, string> attributes, string channel, PrimaryTimesheetPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Channel { get; set; } = channel;
    public PrimaryTimesheetPlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary timesheet plan in status created.</summary>
    public static PrimaryTimesheetPlan NewPrimaryTimesheetPlan(string code, string name)
    {
        return new PrimaryTimesheetPlan(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            PrimaryTimesheetPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTimesheetPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTimesheetPlanStatus next)
    {
        switch (Status)
        {
            case PrimaryTimesheetPlanStatus.Created:
                return next == PrimaryTimesheetPlanStatus.Validated || next == PrimaryTimesheetPlanStatus.Published;
            case PrimaryTimesheetPlanStatus.Validated:
                return next == PrimaryTimesheetPlanStatus.Published;
            case PrimaryTimesheetPlanStatus.Published:
                switch (next)
                {
                    case PrimaryTimesheetPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryTimesheetPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary timesheet plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary timesheet plan"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin + Amount, 2);
    }

    public PrimaryTimesheetPlan Copy()
    {
        return new PrimaryTimesheetPlan(Code, Name, Amount, Timezone, Margin, Grade, Attributes, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
