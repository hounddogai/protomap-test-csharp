namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary town hall rule must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTownHallRuleRule
{
    List<string> Check(PrimaryTownHallRule item);
}

public sealed record PrimaryTownHallRuleRequiredSummaryRule(string Message) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTownHallRuleCodePrefixRule(string Prefix) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTownHallRuleChannelLengthRule(int MaxLength) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTownHallRuleRequiredTitleRule(string Message) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTownHallRuleSummaryLengthRule(int MaxLength) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTownHallRuleAllowedChannelRule(List<string> Allowed) : IPrimaryTownHallRuleRule
{
    public List<string> Check(PrimaryTownHallRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary town hall rule.</summary>
public sealed record PrimaryTownHallRuleValidator(List<IPrimaryTownHallRuleRule> Rules)
{
    public List<string> Validate(PrimaryTownHallRule item)
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

    public void RequireValid(PrimaryTownHallRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTownHallRule> items)
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

/// <summary>Functions of primary town hall rule rules.</summary>
public static class PrimaryTownHallRuleRules
{
    public static PrimaryTownHallRuleValidator DefaultPrimaryTownHallRuleValidator()
    {
        return new PrimaryTownHallRuleValidator(
            new List<IPrimaryTownHallRuleRule>
            {
                new PrimaryTownHallRuleRequiredSummaryRule("primary town hall rule needs a value"),
                new PrimaryTownHallRuleCodePrefixRule("REF"),
                new PrimaryTownHallRuleChannelLengthRule(255),
                new PrimaryTownHallRuleRequiredTitleRule("primary town hall rule needs this field"),
                new PrimaryTownHallRuleSummaryLengthRule(16),
                new PrimaryTownHallRuleAllowedChannelRule(new List<string>()),
            });
    }
}
