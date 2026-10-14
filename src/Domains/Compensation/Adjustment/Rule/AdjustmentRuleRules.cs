namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment rule must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentRuleRule
{
    List<string> Check(AdjustmentRule item);
}

public sealed record AdjustmentRuleAllowedSummaryRule(List<string> Allowed) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRuleTitleLengthRule(int MaxLength) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRuleThresholdRangeRule(double Min, double Max) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRuleRequiredLabelRule(string Message) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRuleLabelLengthRule(int MaxLength) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRuleVersionRangeRule(int Min, int Max) : IAdjustmentRuleRule
{
    public List<string> Check(AdjustmentRule item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment rule.</summary>
public sealed record AdjustmentRuleValidator(List<IAdjustmentRuleRule> Rules)
{
    public List<string> Validate(AdjustmentRule item)
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

    public void RequireValid(AdjustmentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentRule> items)
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

/// <summary>Functions of adjustment rule rules.</summary>
public static class AdjustmentRuleRules
{
    public static AdjustmentRuleValidator DefaultAdjustmentRuleValidator()
    {
        return new AdjustmentRuleValidator(
            new List<IAdjustmentRuleRule>
            {
                new AdjustmentRuleAllowedSummaryRule(new List<string>()),
                new AdjustmentRuleTitleLengthRule(64),
                new AdjustmentRuleThresholdRangeRule(4.1, 3229.5),
                new AdjustmentRuleRequiredLabelRule("adjustment rule needs a value"),
                new AdjustmentRuleLabelLengthRule(3600),
                new AdjustmentRuleVersionRangeRule(255, 1000),
            });
    }
}
