namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default reorg tier must pass. Check returns its problems, or none.</summary>
public interface IDefaultReorgTierRule
{
    List<string> Check(DefaultReorgTier item);
}

public sealed record DefaultReorgTierRequiredCodeRule(string Message) : IDefaultReorgTierRule
{
    public List<string> Check(DefaultReorgTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgTierDescriptionLengthRule(int MaxLength) : IDefaultReorgTierRule
{
    public List<string> Check(DefaultReorgTier item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgTierCodePrefixRule(string Prefix) : IDefaultReorgTierRule
{
    public List<string> Check(DefaultReorgTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgTierAllowedJobCodeRule(List<string> Allowed) : IDefaultReorgTierRule
{
    public List<string> Check(DefaultReorgTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default reorg tier.</summary>
public sealed record DefaultReorgTierValidator(List<IDefaultReorgTierRule> Rules)
{
    public List<string> Validate(DefaultReorgTier item)
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

    public void RequireValid(DefaultReorgTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReorgTier> items)
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

/// <summary>Functions of default reorg tier rules.</summary>
public static class DefaultReorgTierRules
{
    public static DefaultReorgTierValidator DefaultDefaultReorgTierValidator()
    {
        return new DefaultReorgTierValidator(
            new List<IDefaultReorgTierRule>
            {
                new DefaultReorgTierRequiredCodeRule("default reorg tier needs this field"),
                new DefaultReorgTierDescriptionLengthRule(3600),
                new DefaultReorgTierCodePrefixRule("X"),
                new DefaultReorgTierAllowedJobCodeRule(new List<string> { "tiered" }),
            });
    }
}
