namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive town hall rule must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTownHallRuleRule
{
    List<string> Check(AdaptiveTownHallRule item);
}

public sealed record AdaptiveTownHallRuleRequiredNameRule(string Message) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallRuleCodePrefixRule(string Prefix) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallRuleNameLengthRule(int MaxLength) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallRuleRevisionRangeRule(int Min, int Max) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTownHallRuleRequiredCodeRule(string Message) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallRuleAllowedDepartmentRule(List<string> Allowed) : IAdaptiveTownHallRuleRule
{
    public List<string> Check(AdaptiveTownHallRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive town hall rule.</summary>
public sealed record AdaptiveTownHallRuleValidator(List<IAdaptiveTownHallRuleRule> Rules)
{
    public List<string> Validate(AdaptiveTownHallRule item)
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

    public void RequireValid(AdaptiveTownHallRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTownHallRule> items)
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

/// <summary>Functions of adaptive town hall rule rules.</summary>
public static class AdaptiveTownHallRuleRules
{
    public static AdaptiveTownHallRuleValidator DefaultAdaptiveTownHallRuleValidator()
    {
        return new AdaptiveTownHallRuleValidator(
            new List<IAdaptiveTownHallRuleRule>
            {
                new AdaptiveTownHallRuleRequiredNameRule("adaptive town hall rule needs a value"),
                new AdaptiveTownHallRuleCodePrefixRule("REF"),
                new AdaptiveTownHallRuleNameLengthRule(64),
                new AdaptiveTownHallRuleRevisionRangeRule(16, 120),
                new AdaptiveTownHallRuleRequiredCodeRule("adaptive town hall rule needs an entry"),
                new AdaptiveTownHallRuleAllowedDepartmentRule(new List<string>()),
            });
    }
}
