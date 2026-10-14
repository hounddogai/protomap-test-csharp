namespace Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>A rule a capped offboarding profile must pass. Check returns its problems, or none.</summary>
public interface ICappedOffboardingProfileRule
{
    List<string> Check(CappedOffboardingProfile item);
}

public sealed record CappedOffboardingProfileCodePrefixRule(string Prefix) : ICappedOffboardingProfileRule
{
    public List<string> Check(CappedOffboardingProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingProfileRequiredCodeRule(string Message) : ICappedOffboardingProfileRule
{
    public List<string> Check(CappedOffboardingProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingProfileRequiredTitleRule(string Message) : ICappedOffboardingProfileRule
{
    public List<string> Check(CappedOffboardingProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingProfileExpiresAtRangeRule(int Min, int Max) : ICappedOffboardingProfileRule
{
    public List<string> Check(CappedOffboardingProfile item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingProfileCeilingRangeRule(double Min, double Max) : ICappedOffboardingProfileRule
{
    public List<string> Check(CappedOffboardingProfile item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped offboarding profile.</summary>
public sealed record CappedOffboardingProfileValidator(List<ICappedOffboardingProfileRule> Rules)
{
    public List<string> Validate(CappedOffboardingProfile item)
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

    public void RequireValid(CappedOffboardingProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOffboardingProfile> items)
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

/// <summary>Functions of capped offboarding profile rules.</summary>
public static class CappedOffboardingProfileRules
{
    public static CappedOffboardingProfileValidator DefaultCappedOffboardingProfileValidator()
    {
        return new CappedOffboardingProfileValidator(
            new List<ICappedOffboardingProfileRule>
            {
                new CappedOffboardingProfileCodePrefixRule("REF"),
                new CappedOffboardingProfileRequiredCodeRule("capped offboarding profile needs a value"),
                new CappedOffboardingProfileRequiredTitleRule("capped offboarding profile needs an entry"),
                new CappedOffboardingProfileExpiresAtRangeRule(64, 80),
                new CappedOffboardingProfileCeilingRangeRule(8.0, 4064.6),
            });
    }
}
