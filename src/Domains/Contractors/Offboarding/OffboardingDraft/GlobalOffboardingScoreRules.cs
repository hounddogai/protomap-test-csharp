namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

/// <summary>A rule a global offboarding score must pass. Check returns its problems, or none.</summary>
public interface IGlobalOffboardingScoreRule
{
    List<string> Check(GlobalOffboardingScore item);
}

public sealed record GlobalOffboardingScoreCodePrefixRule(string Prefix) : IGlobalOffboardingScoreRule
{
    public List<string> Check(GlobalOffboardingScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingScoreTitleLengthRule(int MaxLength) : IGlobalOffboardingScoreRule
{
    public List<string> Check(GlobalOffboardingScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOffboardingScoreCodeLengthRule(int MaxLength) : IGlobalOffboardingScoreRule
{
    public List<string> Check(GlobalOffboardingScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global offboarding score.</summary>
public sealed record GlobalOffboardingScoreValidator(List<IGlobalOffboardingScoreRule> Rules)
{
    public List<string> Validate(GlobalOffboardingScore item)
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

    public void RequireValid(GlobalOffboardingScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalOffboardingScore> items)
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

/// <summary>Functions of global offboarding score rules.</summary>
public static class GlobalOffboardingScoreRules
{
    public static GlobalOffboardingScoreValidator DefaultGlobalOffboardingScoreValidator()
    {
        return new GlobalOffboardingScoreValidator(
            new List<IGlobalOffboardingScoreRule>
            {
                new GlobalOffboardingScoreCodePrefixRule("X"),
                new GlobalOffboardingScoreTitleLengthRule(32),
                new GlobalOffboardingScoreCodeLengthRule(64),
            });
    }
}
