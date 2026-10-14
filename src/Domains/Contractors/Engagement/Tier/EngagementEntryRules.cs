namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement entry must pass. Check returns its problems, or none.</summary>
public interface IEngagementEntryRule
{
    List<string> Check(EngagementEntry item);
}

public sealed record EngagementEntryCodePrefixRule(string Prefix) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementEntryRequiredLabelRule(string Message) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementEntryLabelLengthRule(int MaxLength) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementEntryAllowedCodeRule(List<string> Allowed) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record EngagementEntryRequiredTeamRule(string Message) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementEntryCodeLengthRule(int MaxLength) : IEngagementEntryRule
{
    public List<string> Check(EngagementEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement entry.</summary>
public sealed record EngagementEntryValidator(List<IEngagementEntryRule> Rules)
{
    public List<string> Validate(EngagementEntry item)
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

    public void RequireValid(EngagementEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementEntry> items)
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

/// <summary>Functions of engagement entry rules.</summary>
public static class EngagementEntryRules
{
    public static EngagementEntryValidator DefaultEngagementEntryValidator()
    {
        return new EngagementEntryValidator(
            new List<IEngagementEntryRule>
            {
                new EngagementEntryCodePrefixRule("REF"),
                new EngagementEntryRequiredLabelRule("engagement entry needs a value"),
                new EngagementEntryLabelLengthRule(64),
                new EngagementEntryAllowedCodeRule(new List<string> { "adaptive", "scheduled" }),
                new EngagementEntryRequiredTeamRule("engagement entry needs this field"),
                new EngagementEntryCodeLengthRule(64),
            });
    }
}
