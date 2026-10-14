namespace Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>A rule a rolling nomination threshold must pass. Check returns its problems, or none.</summary>
public interface IRollingNominationThresholdRule
{
    List<string> Check(RollingNominationThreshold item);
}

public sealed record RollingNominationThresholdTitleLengthRule(int MaxLength) : IRollingNominationThresholdRule
{
    public List<string> Check(RollingNominationThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationThresholdLocaleLengthRule(int MaxLength) : IRollingNominationThresholdRule
{
    public List<string> Check(RollingNominationThreshold item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationThresholdCodePrefixRule(string Prefix) : IRollingNominationThresholdRule
{
    public List<string> Check(RollingNominationThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationThresholdCategoryLengthRule(int MaxLength) : IRollingNominationThresholdRule
{
    public List<string> Check(RollingNominationThreshold item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingNominationThresholdRequiredCodeRule(string Message) : IRollingNominationThresholdRule
{
    public List<string> Check(RollingNominationThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling nomination threshold.</summary>
public sealed record RollingNominationThresholdValidator(List<IRollingNominationThresholdRule> Rules)
{
    public List<string> Validate(RollingNominationThreshold item)
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

    public void RequireValid(RollingNominationThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingNominationThreshold> items)
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

/// <summary>Functions of rolling nomination threshold rules.</summary>
public static class RollingNominationThresholdRules
{
    public static RollingNominationThresholdValidator DefaultRollingNominationThresholdValidator()
    {
        return new RollingNominationThresholdValidator(
            new List<IRollingNominationThresholdRule>
            {
                new RollingNominationThresholdTitleLengthRule(86400),
                new RollingNominationThresholdLocaleLengthRule(32),
                new RollingNominationThresholdCodePrefixRule("X"),
                new RollingNominationThresholdCategoryLengthRule(80),
                new RollingNominationThresholdRequiredCodeRule("rolling nomination threshold needs a value"),
            });
    }
}
