namespace Acme.Workforce.Contractors.Rate.Segment;

/// <summary>A rule a rate assignment must pass. Check returns its problems, or none.</summary>
public interface IRateAssignmentRule
{
    List<string> Check(RateAssignment item);
}

public sealed record RateAssignmentRequiredChecksumRule(string Message) : IRateAssignmentRule
{
    public List<string> Check(RateAssignment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateAssignmentRequiredTitleRule(string Message) : IRateAssignmentRule
{
    public List<string> Check(RateAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateAssignmentRequiredSkillRule(string Message) : IRateAssignmentRule
{
    public List<string> Check(RateAssignment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateAssignmentAmountRangeRule(double Min, double Max) : IRateAssignmentRule
{
    public List<string> Check(RateAssignment item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateAssignmentSkillLengthRule(int MaxLength) : IRateAssignmentRule
{
    public List<string> Check(RateAssignment item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate assignment.</summary>
public sealed record RateAssignmentValidator(List<IRateAssignmentRule> Rules)
{
    public List<string> Validate(RateAssignment item)
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

    public void RequireValid(RateAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateAssignment> items)
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

/// <summary>Functions of rate assignment rules.</summary>
public static class RateAssignmentRules
{
    public static RateAssignmentValidator DefaultRateAssignmentValidator()
    {
        return new RateAssignmentValidator(
            new List<IRateAssignmentRule>
            {
                new RateAssignmentRequiredChecksumRule("rate assignment needs this field"),
                new RateAssignmentRequiredTitleRule("rate assignment needs this field"),
                new RateAssignmentRequiredSkillRule("rate assignment needs a value"),
                new RateAssignmentAmountRangeRule(9.3, 3784.3),
                new RateAssignmentSkillLengthRule(1000),
            });
    }
}
