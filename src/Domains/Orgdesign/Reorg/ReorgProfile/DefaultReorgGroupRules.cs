namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default reorg group must pass. Check returns its problems, or none.</summary>
public interface IDefaultReorgGroupRule
{
    List<string> Check(DefaultReorgGroup item);
}

public sealed record DefaultReorgGroupAllowedLabelRule(List<string> Allowed) : IDefaultReorgGroupRule
{
    public List<string> Check(DefaultReorgGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgGroupRequiredCodeRule(string Message) : IDefaultReorgGroupRule
{
    public List<string> Check(DefaultReorgGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgGroupRequiredLabelRule(string Message) : IDefaultReorgGroupRule
{
    public List<string> Check(DefaultReorgGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgGroupCodePrefixRule(string Prefix) : IDefaultReorgGroupRule
{
    public List<string> Check(DefaultReorgGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgGroupCodeLengthRule(int MaxLength) : IDefaultReorgGroupRule
{
    public List<string> Check(DefaultReorgGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default reorg group.</summary>
public sealed record DefaultReorgGroupValidator(List<IDefaultReorgGroupRule> Rules)
{
    public List<string> Validate(DefaultReorgGroup item)
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

    public void RequireValid(DefaultReorgGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReorgGroup> items)
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

/// <summary>Functions of default reorg group rules.</summary>
public static class DefaultReorgGroupRules
{
    public static DefaultReorgGroupValidator DefaultDefaultReorgGroupValidator()
    {
        return new DefaultReorgGroupValidator(
            new List<IDefaultReorgGroupRule>
            {
                new DefaultReorgGroupAllowedLabelRule(new List<string>()),
                new DefaultReorgGroupRequiredCodeRule("default reorg group needs an entry"),
                new DefaultReorgGroupRequiredLabelRule("default reorg group needs a value"),
                new DefaultReorgGroupCodePrefixRule("ACM"),
                new DefaultReorgGroupCodeLengthRule(80),
            });
    }
}
