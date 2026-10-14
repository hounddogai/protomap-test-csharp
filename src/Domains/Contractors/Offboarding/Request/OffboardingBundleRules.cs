namespace Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>A rule a offboarding bundle must pass. Check returns its problems, or none.</summary>
public interface IOffboardingBundleRule
{
    List<string> Check(OffboardingBundle item);
}

public sealed record OffboardingBundleRequiredSalaryBandRule(string Message) : IOffboardingBundleRule
{
    public List<string> Check(OffboardingBundle item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBundleCodePrefixRule(string Prefix) : IOffboardingBundleRule
{
    public List<string> Check(OffboardingBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingBundleRateRangeRule(double Min, double Max) : IOffboardingBundleRule
{
    public List<string> Check(OffboardingBundle item)
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

public sealed record OffboardingBundleNameLengthRule(int MaxLength) : IOffboardingBundleRule
{
    public List<string> Check(OffboardingBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding bundle.</summary>
public sealed record OffboardingBundleValidator(List<IOffboardingBundleRule> Rules)
{
    public List<string> Validate(OffboardingBundle item)
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

    public void RequireValid(OffboardingBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingBundle> items)
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

/// <summary>Functions of offboarding bundle rules.</summary>
public static class OffboardingBundleRules
{
    public static OffboardingBundleValidator DefaultOffboardingBundleValidator()
    {
        return new OffboardingBundleValidator(
            new List<IOffboardingBundleRule>
            {
                new OffboardingBundleRequiredSalaryBandRule("offboarding bundle needs an entry"),
                new OffboardingBundleCodePrefixRule("OPS"),
                new OffboardingBundleRateRangeRule(9.6, 620.3),
                new OffboardingBundleNameLengthRule(86400),
            });
    }
}
