namespace Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>A rule a timesheet reservation must pass. Check returns its problems, or none.</summary>
public interface ITimesheetReservationRule
{
    List<string> Check(TimesheetReservation item);
}

public sealed record TimesheetReservationLabelLengthRule(int MaxLength) : ITimesheetReservationRule
{
    public List<string> Check(TimesheetReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReservationStartsAtRangeRule(int Min, int Max) : ITimesheetReservationRule
{
    public List<string> Check(TimesheetReservation item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReservationNameLengthRule(int MaxLength) : ITimesheetReservationRule
{
    public List<string> Check(TimesheetReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetReservationRequiredNameRule(string Message) : ITimesheetReservationRule
{
    public List<string> Check(TimesheetReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet reservation.</summary>
public sealed record TimesheetReservationValidator(List<ITimesheetReservationRule> Rules)
{
    public List<string> Validate(TimesheetReservation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(TimesheetReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetReservation> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of timesheet reservation rules.</summary>
public static class TimesheetReservationRules
{
    public static TimesheetReservationValidator DefaultTimesheetReservationValidator()
    {
        return new TimesheetReservationValidator(
            new List<ITimesheetReservationRule>
            {
                new TimesheetReservationLabelLengthRule(16),
                new TimesheetReservationStartsAtRangeRule(16, 64),
                new TimesheetReservationNameLengthRule(120),
                new TimesheetReservationRequiredNameRule("timesheet reservation needs a value"),
            });
    }
}
