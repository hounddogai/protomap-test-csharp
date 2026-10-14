namespace Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>A rule a fallback pool assignment must pass. Check returns its problems, or none.</summary>
public interface IFallbackPoolAssignmentRule
{
    List<string> Check(FallbackPoolAssignment item);
}

public sealed record FallbackPoolAssignmentRequiredNameRule(string Message) : IFallbackPoolAssignmentRule
{
    public List<string> Check(FallbackPoolAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolAssignmentMinimumRangeRule(double Min, double Max) : IFallbackPoolAssignmentRule
{
    public List<string> Check(FallbackPoolAssignment item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPoolAssignmentCodeLengthRule(int MaxLength) : IFallbackPoolAssignmentRule
{
    public List<string> Check(FallbackPoolAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolAssignmentRequiredCodeRule(string Message) : IFallbackPoolAssignmentRule
{
    public List<string> Check(FallbackPoolAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pool assignment.</summary>
public sealed record FallbackPoolAssignmentValidator(List<IFallbackPoolAssignmentRule> Rules)
{
    public List<string> Validate(FallbackPoolAssignment item)
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

    public void RequireValid(FallbackPoolAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPoolAssignment> items)
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

/// <summary>Functions of fallback pool assignment rules.</summary>
public static class FallbackPoolAssignmentRules
{
    public static FallbackPoolAssignmentValidator DefaultFallbackPoolAssignmentValidator()
    {
        return new FallbackPoolAssignmentValidator(
            new List<IFallbackPoolAssignmentRule>
            {
                new FallbackPoolAssignmentRequiredNameRule("fallback pool assignment needs this field"),
                new FallbackPoolAssignmentMinimumRangeRule(4.0, 4434.1),
                new FallbackPoolAssignmentCodeLengthRule(64),
                new FallbackPoolAssignmentRequiredCodeRule("fallback pool assignment needs this field"),
            });
    }
}
