namespace Acme.Workforce.Contractors.Rate.Window;

/// <summary>A rule a fallback rate bundle must pass. Check returns its problems, or none.</summary>
public interface IFallbackRateBundleRule
{
    List<string> Check(FallbackRateBundle item);
}

public sealed record FallbackRateBundleRequiredChecksumRule(string Message) : IFallbackRateBundleRule
{
    public List<string> Check(FallbackRateBundle item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateBundleEndsAtRangeRule(int Min, int Max) : IFallbackRateBundleRule
{
    public List<string> Check(FallbackRateBundle item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateBundleCodeLengthRule(int MaxLength) : IFallbackRateBundleRule
{
    public List<string> Check(FallbackRateBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateBundleRequiredManagerIdRule(string Message) : IFallbackRateBundleRule
{
    public List<string> Check(FallbackRateBundle item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateBundleMinimumRangeRule(double Min, double Max) : IFallbackRateBundleRule
{
    public List<string> Check(FallbackRateBundle item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback rate bundle.</summary>
public sealed record FallbackRateBundleValidator(List<IFallbackRateBundleRule> Rules)
{
    public List<string> Validate(FallbackRateBundle item)
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

    public void RequireValid(FallbackRateBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRateBundle> items)
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

/// <summary>Functions of fallback rate bundle rules.</summary>
public static class FallbackRateBundleRules
{
    public static FallbackRateBundleValidator DefaultFallbackRateBundleValidator()
    {
        return new FallbackRateBundleValidator(
            new List<IFallbackRateBundleRule>
            {
                new FallbackRateBundleRequiredChecksumRule("fallback rate bundle needs this field"),
                new FallbackRateBundleEndsAtRangeRule(32, 86400),
                new FallbackRateBundleCodeLengthRule(1000),
                new FallbackRateBundleRequiredManagerIdRule("fallback rate bundle needs an entry"),
                new FallbackRateBundleMinimumRangeRule(6.5, 627.1),
            });
    }
}
