namespace Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>A rule a career path policy must pass. Check returns its problems, or none.</summary>
public interface ICareerPathPolicyRule
{
    List<string> Check(CareerPathPolicy item);
}

public sealed record CareerPathPolicyCodeLengthRule(int MaxLength) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPolicyCodePrefixRule(string Prefix) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPolicySegmentLengthRule(int MaxLength) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPolicyRequiredCodeRule(string Message) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPolicyLocationLengthRule(int MaxLength) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathPolicyEndsAtRangeRule(int Min, int Max) : ICareerPathPolicyRule
{
    public List<string> Check(CareerPathPolicy item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a career path policy.</summary>
public sealed record CareerPathPolicyValidator(List<ICareerPathPolicyRule> Rules)
{
    public List<string> Validate(CareerPathPolicy item)
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

    public void RequireValid(CareerPathPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathPolicy> items)
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

/// <summary>Functions of career path policy rules.</summary>
public static class CareerPathPolicyRules
{
    public static CareerPathPolicyValidator DefaultCareerPathPolicyValidator()
    {
        return new CareerPathPolicyValidator(
            new List<ICareerPathPolicyRule>
            {
                new CareerPathPolicyCodeLengthRule(64),
                new CareerPathPolicyCodePrefixRule("ACM"),
                new CareerPathPolicySegmentLengthRule(1000),
                new CareerPathPolicyRequiredCodeRule("career path policy needs a value"),
                new CareerPathPolicyLocationLengthRule(16),
                new CareerPathPolicyEndsAtRangeRule(64, 120),
            });
    }
}
