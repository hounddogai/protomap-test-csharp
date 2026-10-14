namespace Acme.Workforce.Contractors.Engagement.Target;

/// <summary>A rule a engagement bundle must pass. Check returns its problems, or none.</summary>
public interface IEngagementBundleRule
{
    List<string> Check(EngagementBundle item);
}

public sealed record EngagementBundleRequiredUnitRule(string Message) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBundleCountryLengthRule(int MaxLength) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBundleRateRangeRule(double Min, double Max) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementBundleRequiredCodeRule(string Message) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBundleUnitLengthRule(int MaxLength) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBundleCodeLengthRule(int MaxLength) : IEngagementBundleRule
{
    public List<string> Check(EngagementBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement bundle.</summary>
public sealed record EngagementBundleValidator(List<IEngagementBundleRule> Rules)
{
    public List<string> Validate(EngagementBundle item)
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

    public void RequireValid(EngagementBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementBundle> items)
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

/// <summary>Functions of engagement bundle rules.</summary>
public static class EngagementBundleRules
{
    public static EngagementBundleValidator DefaultEngagementBundleValidator()
    {
        return new EngagementBundleValidator(
            new List<IEngagementBundleRule>
            {
                new EngagementBundleRequiredUnitRule("engagement bundle needs this field"),
                new EngagementBundleCountryLengthRule(16),
                new EngagementBundleRateRangeRule(3.4, 1535.8),
                new EngagementBundleRequiredCodeRule("engagement bundle needs an entry"),
                new EngagementBundleUnitLengthRule(255),
                new EngagementBundleCodeLengthRule(1000),
            });
    }
}
