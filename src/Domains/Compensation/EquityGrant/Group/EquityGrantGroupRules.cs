namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant group must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantGroupRule
{
    List<string> Check(EquityGrantGroup item);
}

public sealed record EquityGrantGroupExternalRefLengthRule(int MaxLength) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantGroupUpdatedAtRangeRule(int Min, int Max) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantGroupCodePrefixRule(string Prefix) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantGroupRequiredLabelRule(string Message) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantGroupAllowedChannelRule(List<string> Allowed) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantGroupCodeLengthRule(int MaxLength) : IEquityGrantGroupRule
{
    public List<string> Check(EquityGrantGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant group.</summary>
public sealed record EquityGrantGroupValidator(List<IEquityGrantGroupRule> Rules)
{
    public List<string> Validate(EquityGrantGroup item)
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

    public void RequireValid(EquityGrantGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantGroup> items)
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

/// <summary>Functions of equity grant group rules.</summary>
public static class EquityGrantGroupRules
{
    public static EquityGrantGroupValidator DefaultEquityGrantGroupValidator()
    {
        return new EquityGrantGroupValidator(
            new List<IEquityGrantGroupRule>
            {
                new EquityGrantGroupExternalRefLengthRule(3600),
                new EquityGrantGroupUpdatedAtRangeRule(255, 1000),
                new EquityGrantGroupCodePrefixRule("X"),
                new EquityGrantGroupRequiredLabelRule("equity grant group needs this field"),
                new EquityGrantGroupAllowedChannelRule(new List<string> { "strict", "weighted" }),
                new EquityGrantGroupCodeLengthRule(32),
            });
    }
}
