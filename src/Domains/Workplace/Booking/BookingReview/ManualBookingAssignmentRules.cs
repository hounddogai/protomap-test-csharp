namespace Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>A rule a manual booking assignment must pass. Check returns its problems, or none.</summary>
public interface IManualBookingAssignmentRule
{
    List<string> Check(ManualBookingAssignment item);
}

public sealed record ManualBookingAssignmentRequiredRegionRule(string Message) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAssignmentVersionRangeRule(int Min, int Max) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAssignmentNameLengthRule(int MaxLength) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAssignmentWeightRangeRule(double Min, double Max) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualBookingAssignmentRequiredNameRule(string Message) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingAssignmentPriorityRangeRule(int Min, int Max) : IManualBookingAssignmentRule
{
    public List<string> Check(ManualBookingAssignment item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual booking assignment.</summary>
public sealed record ManualBookingAssignmentValidator(List<IManualBookingAssignmentRule> Rules)
{
    public List<string> Validate(ManualBookingAssignment item)
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

    public void RequireValid(ManualBookingAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBookingAssignment> items)
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

/// <summary>Functions of manual booking assignment rules.</summary>
public static class ManualBookingAssignmentRules
{
    public static ManualBookingAssignmentValidator DefaultManualBookingAssignmentValidator()
    {
        return new ManualBookingAssignmentValidator(
            new List<IManualBookingAssignmentRule>
            {
                new ManualBookingAssignmentRequiredRegionRule("manual booking assignment needs a value"),
                new ManualBookingAssignmentVersionRangeRule(64, 80),
                new ManualBookingAssignmentNameLengthRule(120),
                new ManualBookingAssignmentWeightRangeRule(5.0, 4515.6),
                new ManualBookingAssignmentRequiredNameRule("manual booking assignment needs a value"),
                new ManualBookingAssignmentPriorityRangeRule(32, 80),
            });
    }
}
