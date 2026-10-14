namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>A rule a tiered booking assignment must pass. Check returns its problems, or none.</summary>
public interface ITieredBookingAssignmentRule
{
    List<string> Check(TieredBookingAssignment item);
}

public sealed record TieredBookingAssignmentRequiredLabelRule(string Message) : ITieredBookingAssignmentRule
{
    public List<string> Check(TieredBookingAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingAssignmentRequiredCodeRule(string Message) : ITieredBookingAssignmentRule
{
    public List<string> Check(TieredBookingAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingAssignmentOwnerIdLengthRule(int MaxLength) : ITieredBookingAssignmentRule
{
    public List<string> Check(TieredBookingAssignment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered booking assignment.</summary>
public sealed record TieredBookingAssignmentValidator(List<ITieredBookingAssignmentRule> Rules)
{
    public List<string> Validate(TieredBookingAssignment item)
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

    public void RequireValid(TieredBookingAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBookingAssignment> items)
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

/// <summary>Functions of tiered booking assignment rules.</summary>
public static class TieredBookingAssignmentRules
{
    public static TieredBookingAssignmentValidator DefaultTieredBookingAssignmentValidator()
    {
        return new TieredBookingAssignmentValidator(
            new List<ITieredBookingAssignmentRule>
            {
                new TieredBookingAssignmentRequiredLabelRule("tiered booking assignment needs an entry"),
                new TieredBookingAssignmentRequiredCodeRule("tiered booking assignment needs this field"),
                new TieredBookingAssignmentOwnerIdLengthRule(32),
            });
    }
}
