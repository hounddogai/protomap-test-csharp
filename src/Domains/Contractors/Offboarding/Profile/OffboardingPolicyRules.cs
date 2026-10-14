namespace Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>A rule a offboarding policy must pass. Check returns its problems, or none.</summary>
public interface IOffboardingPolicyRule
{
    List<string> Check(OffboardingPolicy item);
}

public sealed record OffboardingPolicyRequiredTeamRule(string Message) : IOffboardingPolicyRule
{
    public List<string> Check(OffboardingPolicy item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPolicyRatioRangeRule(double Min, double Max) : IOffboardingPolicyRule
{
    public List<string> Check(OffboardingPolicy item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingPolicyCodePrefixRule(string Prefix) : IOffboardingPolicyRule
{
    public List<string> Check(OffboardingPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPolicyMaximumRangeRule(double Min, double Max) : IOffboardingPolicyRule
{
    public List<string> Check(OffboardingPolicy item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingPolicyTitleLengthRule(int MaxLength) : IOffboardingPolicyRule
{
    public List<string> Check(OffboardingPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding policy.</summary>
public sealed record OffboardingPolicyValidator(List<IOffboardingPolicyRule> Rules)
{
    public List<string> Validate(OffboardingPolicy item)
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

    public void RequireValid(OffboardingPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingPolicy> items)
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

/// <summary>Functions of offboarding policy rules.</summary>
public static class OffboardingPolicyRules
{
    public static OffboardingPolicyValidator DefaultOffboardingPolicyValidator()
    {
        return new OffboardingPolicyValidator(
            new List<IOffboardingPolicyRule>
            {
                new OffboardingPolicyRequiredTeamRule("offboarding policy needs an entry"),
                new OffboardingPolicyRatioRangeRule(5.2, 4293.3),
                new OffboardingPolicyCodePrefixRule("ACM"),
                new OffboardingPolicyMaximumRangeRule(3.5, 2658.0),
                new OffboardingPolicyTitleLengthRule(80),
            });
    }
}
