namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>A rule a career path threshold must pass. Check returns its problems, or none.</summary>
public interface ICareerPathThresholdRule
{
    List<string> Check(CareerPathThreshold item);
}

public sealed record CareerPathThresholdCreatedAtRangeRule(int Min, int Max) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathThresholdRequiredCodeRule(string Message) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathThresholdRequiredBadgeRule(string Message) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathThresholdRequiredSummaryRule(string Message) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathThresholdCodePrefixRule(string Prefix) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathThresholdCodeLengthRule(int MaxLength) : ICareerPathThresholdRule
{
    public List<string> Check(CareerPathThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path threshold.</summary>
public sealed record CareerPathThresholdValidator(List<ICareerPathThresholdRule> Rules)
{
    public List<string> Validate(CareerPathThreshold item)
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

    public void RequireValid(CareerPathThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathThreshold> items)
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

/// <summary>Functions of career path threshold rules.</summary>
public static class CareerPathThresholdRules
{
    public static CareerPathThresholdValidator DefaultCareerPathThresholdValidator()
    {
        return new CareerPathThresholdValidator(
            new List<ICareerPathThresholdRule>
            {
                new CareerPathThresholdCreatedAtRangeRule(16, 1000),
                new CareerPathThresholdRequiredCodeRule("career path threshold needs a value"),
                new CareerPathThresholdRequiredBadgeRule("career path threshold needs an entry"),
                new CareerPathThresholdRequiredSummaryRule("career path threshold needs an entry"),
                new CareerPathThresholdCodePrefixRule("OPS"),
                new CareerPathThresholdCodeLengthRule(64),
            });
    }
}
