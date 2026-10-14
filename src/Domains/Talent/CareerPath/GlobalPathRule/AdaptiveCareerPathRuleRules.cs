namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive career path rule must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCareerPathRuleRule
{
    List<string> Check(AdaptiveCareerPathRule item);
}

public sealed record AdaptiveCareerPathRuleQuantityRangeRule(int Min, int Max) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveCareerPathRuleRequiredLabelRule(string Message) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathRuleRequiredCodeRule(string Message) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathRuleCodePrefixRule(string Prefix) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathRuleLabelLengthRule(int MaxLength) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathRuleAllowedLabelRule(List<string> Allowed) : IAdaptiveCareerPathRuleRule
{
    public List<string> Check(AdaptiveCareerPathRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive career path rule.</summary>
public sealed record AdaptiveCareerPathRuleValidator(List<IAdaptiveCareerPathRuleRule> Rules)
{
    public List<string> Validate(AdaptiveCareerPathRule item)
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

    public void RequireValid(AdaptiveCareerPathRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCareerPathRule> items)
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

/// <summary>Functions of adaptive career path rule rules.</summary>
public static class AdaptiveCareerPathRuleRules
{
    public static AdaptiveCareerPathRuleValidator DefaultAdaptiveCareerPathRuleValidator()
    {
        return new AdaptiveCareerPathRuleValidator(
            new List<IAdaptiveCareerPathRuleRule>
            {
                new AdaptiveCareerPathRuleQuantityRangeRule(255, 3600),
                new AdaptiveCareerPathRuleRequiredLabelRule("adaptive career path rule needs an entry"),
                new AdaptiveCareerPathRuleRequiredCodeRule("adaptive career path rule needs an entry"),
                new AdaptiveCareerPathRuleCodePrefixRule("OPS"),
                new AdaptiveCareerPathRuleLabelLengthRule(1000),
                new AdaptiveCareerPathRuleAllowedLabelRule(new List<string> { "scheduled", "weighted" }),
            });
    }
}
