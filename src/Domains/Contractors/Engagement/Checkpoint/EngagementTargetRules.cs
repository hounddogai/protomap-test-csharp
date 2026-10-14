namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>A rule a engagement target must pass. Check returns its problems, or none.</summary>
public interface IEngagementTargetRule
{
    List<string> Check(EngagementTarget item);
}

public sealed record EngagementTargetRequiredCodeRule(string Message) : IEngagementTargetRule
{
    public List<string> Check(EngagementTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementTargetNameLengthRule(int MaxLength) : IEngagementTargetRule
{
    public List<string> Check(EngagementTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTargetCodePrefixRule(string Prefix) : IEngagementTargetRule
{
    public List<string> Check(EngagementTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTargetRequiredLocaleRule(string Message) : IEngagementTargetRule
{
    public List<string> Check(EngagementTarget item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement target.</summary>
public sealed record EngagementTargetValidator(List<IEngagementTargetRule> Rules)
{
    public List<string> Validate(EngagementTarget item)
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

    public void RequireValid(EngagementTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementTarget> items)
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

/// <summary>Functions of engagement target rules.</summary>
public static class EngagementTargetRules
{
    public static EngagementTargetValidator DefaultEngagementTargetValidator()
    {
        return new EngagementTargetValidator(
            new List<IEngagementTargetRule>
            {
                new EngagementTargetRequiredCodeRule("engagement target needs this field"),
                new EngagementTargetNameLengthRule(32),
                new EngagementTargetCodePrefixRule("X"),
                new EngagementTargetRequiredLocaleRule("engagement target needs an entry"),
            });
    }
}
