namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted reorg rule must pass. Check returns its problems, or none.</summary>
public interface IWeightedReorgRuleRule
{
    List<string> Check(WeightedReorgRule item);
}

public sealed record WeightedReorgRuleAllowedCodeRule(List<string> Allowed) : IWeightedReorgRuleRule
{
    public List<string> Check(WeightedReorgRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgRuleRequiredCodeRule(string Message) : IWeightedReorgRuleRule
{
    public List<string> Check(WeightedReorgRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgRuleCodePrefixRule(string Prefix) : IWeightedReorgRuleRule
{
    public List<string> Check(WeightedReorgRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgRuleVersionRangeRule(int Min, int Max) : IWeightedReorgRuleRule
{
    public List<string> Check(WeightedReorgRule item)
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

/// <summary>Runs every rule of a weighted reorg rule.</summary>
public sealed record WeightedReorgRuleValidator(List<IWeightedReorgRuleRule> Rules)
{
    public List<string> Validate(WeightedReorgRule item)
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

    public void RequireValid(WeightedReorgRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReorgRule> items)
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

/// <summary>Functions of weighted reorg rule rules.</summary>
public static class WeightedReorgRuleRules
{
    public static WeightedReorgRuleValidator DefaultWeightedReorgRuleValidator()
    {
        return new WeightedReorgRuleValidator(
            new List<IWeightedReorgRuleRule>
            {
                new WeightedReorgRuleAllowedCodeRule(new List<string> { "legacy", "tiered" }),
                new WeightedReorgRuleRequiredCodeRule("weighted reorg rule needs this field"),
                new WeightedReorgRuleCodePrefixRule("Q"),
                new WeightedReorgRuleVersionRangeRule(255, 3600),
            });
    }
}
