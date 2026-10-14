namespace Acme.Workforce.Compliance.Exception.Rule;

/// <summary>A rule a fallback exception assignment must pass. Check returns its problems, or none.</summary>
public interface IFallbackExceptionAssignmentRule
{
    List<string> Check(FallbackExceptionAssignment item);
}

public sealed record FallbackExceptionAssignmentFteRangeRule(double Min, double Max) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionAssignmentCodePrefixRule(string Prefix) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionAssignmentCostRangeRule(double Min, double Max) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionAssignmentNameLengthRule(int MaxLength) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionAssignmentRequiredCodeRule(string Message) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionAssignmentVersionRangeRule(int Min, int Max) : IFallbackExceptionAssignmentRule
{
    public List<string> Check(FallbackExceptionAssignment item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback exception assignment.</summary>
public sealed record FallbackExceptionAssignmentValidator(List<IFallbackExceptionAssignmentRule> Rules)
{
    public List<string> Validate(FallbackExceptionAssignment item)
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

    public void RequireValid(FallbackExceptionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackExceptionAssignment> items)
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

/// <summary>Functions of fallback exception assignment rules.</summary>
public static class FallbackExceptionAssignmentRules
{
    public static FallbackExceptionAssignmentValidator DefaultFallbackExceptionAssignmentValidator()
    {
        return new FallbackExceptionAssignmentValidator(
            new List<IFallbackExceptionAssignmentRule>
            {
                new FallbackExceptionAssignmentFteRangeRule(6.5, 1920.1),
                new FallbackExceptionAssignmentCodePrefixRule("REF"),
                new FallbackExceptionAssignmentCostRangeRule(9.5, 4587.7),
                new FallbackExceptionAssignmentNameLengthRule(16),
                new FallbackExceptionAssignmentRequiredCodeRule("fallback exception assignment needs a value"),
                new FallbackExceptionAssignmentVersionRangeRule(120, 1000),
            });
    }
}
