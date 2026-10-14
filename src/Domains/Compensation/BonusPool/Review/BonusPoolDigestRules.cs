namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool digest must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolDigestRule
{
    List<string> Check(BonusPoolDigest item);
}

public sealed record BonusPoolDigestCodePrefixRule(string Prefix) : IBonusPoolDigestRule
{
    public List<string> Check(BonusPoolDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolDigestAllowedNameRule(List<string> Allowed) : IBonusPoolDigestRule
{
    public List<string> Check(BonusPoolDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolDigestVersionRangeRule(int Min, int Max) : IBonusPoolDigestRule
{
    public List<string> Check(BonusPoolDigest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolDigestRequiredCodeRule(string Message) : IBonusPoolDigestRule
{
    public List<string> Check(BonusPoolDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolDigestRequiredNameRule(string Message) : IBonusPoolDigestRule
{
    public List<string> Check(BonusPoolDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool digest.</summary>
public sealed record BonusPoolDigestValidator(List<IBonusPoolDigestRule> Rules)
{
    public List<string> Validate(BonusPoolDigest item)
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

    public void RequireValid(BonusPoolDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolDigest> items)
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

/// <summary>Functions of bonus pool digest rules.</summary>
public static class BonusPoolDigestRules
{
    public static BonusPoolDigestValidator DefaultBonusPoolDigestValidator()
    {
        return new BonusPoolDigestValidator(
            new List<IBonusPoolDigestRule>
            {
                new BonusPoolDigestCodePrefixRule("REF"),
                new BonusPoolDigestAllowedNameRule(new List<string> { "global" }),
                new BonusPoolDigestVersionRangeRule(1000, 3600),
                new BonusPoolDigestRequiredCodeRule("bonus pool digest needs this field"),
                new BonusPoolDigestRequiredNameRule("bonus pool digest needs a value"),
            });
    }
}
