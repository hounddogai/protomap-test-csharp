namespace Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>A rule a position estimate must pass. Check returns its problems, or none.</summary>
public interface IPositionEstimateRule
{
    List<string> Check(PositionEstimate item);
}

public sealed record PositionEstimateCodePrefixRule(string Prefix) : IPositionEstimateRule
{
    public List<string> Check(PositionEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionEstimateNameLengthRule(int MaxLength) : IPositionEstimateRule
{
    public List<string> Check(PositionEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionEstimateCapacityRangeRule(int Min, int Max) : IPositionEstimateRule
{
    public List<string> Check(PositionEstimate item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionEstimateEmployeeIdLengthRule(int MaxLength) : IPositionEstimateRule
{
    public List<string> Check(PositionEstimate item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionEstimateRequiredCodeRule(string Message) : IPositionEstimateRule
{
    public List<string> Check(PositionEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position estimate.</summary>
public sealed record PositionEstimateValidator(List<IPositionEstimateRule> Rules)
{
    public List<string> Validate(PositionEstimate item)
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

    public void RequireValid(PositionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionEstimate> items)
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

/// <summary>Functions of position estimate rules.</summary>
public static class PositionEstimateRules
{
    public static PositionEstimateValidator DefaultPositionEstimateValidator()
    {
        return new PositionEstimateValidator(
            new List<IPositionEstimateRule>
            {
                new PositionEstimateCodePrefixRule("ACM"),
                new PositionEstimateNameLengthRule(16),
                new PositionEstimateCapacityRangeRule(1000, 3600),
                new PositionEstimateEmployeeIdLengthRule(86400),
                new PositionEstimateRequiredCodeRule("position estimate needs an entry"),
            });
    }
}
