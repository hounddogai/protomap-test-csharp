namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement rule must pass. Check returns its problems, or none.</summary>
public interface IEngagementRuleRule
{
    List<string> Check(EngagementRule item);
}

public sealed record EngagementRuleCodePrefixRule(string Prefix) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRuleNameLengthRule(int MaxLength) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRuleRequiredCodeRule(string Message) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementRuleAllowedNameRule(List<string> Allowed) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRuleRetriesRangeRule(int Min, int Max) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRuleAmountRangeRule(double Min, double Max) : IEngagementRuleRule
{
    public List<string> Check(EngagementRule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a engagement rule.</summary>
public sealed record EngagementRuleValidator(List<IEngagementRuleRule> Rules)
{
    public List<string> Validate(EngagementRule item)
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

    public void RequireValid(EngagementRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementRule> items)
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

/// <summary>Functions of engagement rule rules.</summary>
public static class EngagementRuleRules
{
    public static EngagementRuleValidator DefaultEngagementRuleValidator()
    {
        return new EngagementRuleValidator(
            new List<IEngagementRuleRule>
            {
                new EngagementRuleCodePrefixRule("Q"),
                new EngagementRuleNameLengthRule(64),
                new EngagementRuleRequiredCodeRule("engagement rule needs a value"),
                new EngagementRuleAllowedNameRule(new List<string> { "lenient", "pending", "tiered" }),
                new EngagementRuleRetriesRangeRule(1000, 3600),
                new EngagementRuleAmountRangeRule(9.0, 193.0),
            });
    }
}
