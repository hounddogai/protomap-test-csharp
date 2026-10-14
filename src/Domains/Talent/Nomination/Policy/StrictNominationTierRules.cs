namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict nomination tier must pass. Check returns its problems, or none.</summary>
public interface IStrictNominationTierRule
{
    List<string> Check(StrictNominationTier item);
}

public sealed record StrictNominationTierRequiredTitleRule(string Message) : IStrictNominationTierRule
{
    public List<string> Check(StrictNominationTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationTierAllowedCodeRule(List<string> Allowed) : IStrictNominationTierRule
{
    public List<string> Check(StrictNominationTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationTierRequiredCodeRule(string Message) : IStrictNominationTierRule
{
    public List<string> Check(StrictNominationTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationTierCodeLengthRule(int MaxLength) : IStrictNominationTierRule
{
    public List<string> Check(StrictNominationTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationTierRequiredTimezoneRule(string Message) : IStrictNominationTierRule
{
    public List<string> Check(StrictNominationTier item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict nomination tier.</summary>
public sealed record StrictNominationTierValidator(List<IStrictNominationTierRule> Rules)
{
    public List<string> Validate(StrictNominationTier item)
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

    public void RequireValid(StrictNominationTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictNominationTier> items)
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

/// <summary>Functions of strict nomination tier rules.</summary>
public static class StrictNominationTierRules
{
    public static StrictNominationTierValidator DefaultStrictNominationTierValidator()
    {
        return new StrictNominationTierValidator(
            new List<IStrictNominationTierRule>
            {
                new StrictNominationTierRequiredTitleRule("strict nomination tier needs this field"),
                new StrictNominationTierAllowedCodeRule(new List<string> { "seasonal", "default" }),
                new StrictNominationTierRequiredCodeRule("strict nomination tier needs an entry"),
                new StrictNominationTierCodeLengthRule(16),
                new StrictNominationTierRequiredTimezoneRule("strict nomination tier needs an entry"),
            });
    }
}
