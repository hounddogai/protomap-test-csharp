namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>A rule a adjustment assignment must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentAssignmentRule
{
    List<string> Check(AdjustmentAssignment item);
}

public sealed record AdjustmentAssignmentCountryLengthRule(int MaxLength) : IAdjustmentAssignmentRule
{
    public List<string> Check(AdjustmentAssignment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAssignmentRequiredSegmentRule(string Message) : IAdjustmentAssignmentRule
{
    public List<string> Check(AdjustmentAssignment item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAssignmentRequiredNameRule(string Message) : IAdjustmentAssignmentRule
{
    public List<string> Check(AdjustmentAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment assignment.</summary>
public sealed record AdjustmentAssignmentValidator(List<IAdjustmentAssignmentRule> Rules)
{
    public List<string> Validate(AdjustmentAssignment item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(AdjustmentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentAssignment> items)
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

/// <summary>Functions of adjustment assignment rules.</summary>
public static class AdjustmentAssignmentRules
{
    public static AdjustmentAssignmentValidator DefaultAdjustmentAssignmentValidator()
    {
        return new AdjustmentAssignmentValidator(
            new List<IAdjustmentAssignmentRule>
            {
                new AdjustmentAssignmentCountryLengthRule(3600),
                new AdjustmentAssignmentRequiredSegmentRule("adjustment assignment needs an entry"),
                new AdjustmentAssignmentRequiredNameRule("adjustment assignment needs an entry"),
            });
    }
}
