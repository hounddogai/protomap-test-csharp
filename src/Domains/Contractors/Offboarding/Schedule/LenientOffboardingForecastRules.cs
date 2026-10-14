namespace Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>A rule a lenient offboarding forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientOffboardingForecastRule
{
    List<string> Check(LenientOffboardingForecast item);
}

public sealed record LenientOffboardingForecastLocaleLengthRule(int MaxLength) : ILenientOffboardingForecastRule
{
    public List<string> Check(LenientOffboardingForecast item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingForecastRequiredNameRule(string Message) : ILenientOffboardingForecastRule
{
    public List<string> Check(LenientOffboardingForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingForecastNameLengthRule(int MaxLength) : ILenientOffboardingForecastRule
{
    public List<string> Check(LenientOffboardingForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingForecastCodePrefixRule(string Prefix) : ILenientOffboardingForecastRule
{
    public List<string> Check(LenientOffboardingForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingForecastCodeLengthRule(int MaxLength) : ILenientOffboardingForecastRule
{
    public List<string> Check(LenientOffboardingForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient offboarding forecast.</summary>
public sealed record LenientOffboardingForecastValidator(List<ILenientOffboardingForecastRule> Rules)
{
    public List<string> Validate(LenientOffboardingForecast item)
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

    public void RequireValid(LenientOffboardingForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientOffboardingForecast> items)
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

/// <summary>Functions of lenient offboarding forecast rules.</summary>
public static class LenientOffboardingForecastRules
{
    public static LenientOffboardingForecastValidator DefaultLenientOffboardingForecastValidator()
    {
        return new LenientOffboardingForecastValidator(
            new List<ILenientOffboardingForecastRule>
            {
                new LenientOffboardingForecastLocaleLengthRule(32),
                new LenientOffboardingForecastRequiredNameRule("lenient offboarding forecast needs an entry"),
                new LenientOffboardingForecastNameLengthRule(1000),
                new LenientOffboardingForecastCodePrefixRule("ACM"),
                new LenientOffboardingForecastCodeLengthRule(120),
            });
    }
}
