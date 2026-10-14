namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg rule must pass. Check returns its problems, or none.</summary>
public interface IReorgRuleRule
{
    List<string> Check(ReorgRule item);
}

public sealed record ReorgRuleRequiredCodeRule(string Message) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgRuleSkillLengthRule(int MaxLength) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRuleCostRangeRule(double Min, double Max) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRuleRequiredNameRule(string Message) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgRuleAllowedSkillRule(List<string> Allowed) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ReorgRuleCodePrefixRule(string Prefix) : IReorgRuleRule
{
    public List<string> Check(ReorgRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg rule.</summary>
public sealed record ReorgRuleValidator(List<IReorgRuleRule> Rules)
{
    public List<string> Validate(ReorgRule item)
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

    public void RequireValid(ReorgRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgRule> items)
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

/// <summary>Functions of reorg rule rules.</summary>
public static class ReorgRuleRules
{
    public static ReorgRuleValidator DefaultReorgRuleValidator()
    {
        return new ReorgRuleValidator(
            new List<IReorgRuleRule>
            {
                new ReorgRuleRequiredCodeRule("reorg rule needs a value"),
                new ReorgRuleSkillLengthRule(16),
                new ReorgRuleCostRangeRule(8.2, 1173.1),
                new ReorgRuleRequiredNameRule("reorg rule needs a value"),
                new ReorgRuleAllowedSkillRule(new List<string>()),
                new ReorgRuleCodePrefixRule("X"),
            });
    }
}
