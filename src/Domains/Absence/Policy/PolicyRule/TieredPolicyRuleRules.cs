namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered policy rule must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyRuleRule
{
    List<string> Check(TieredPolicyRule item);
}

public sealed record TieredPolicyRuleAllowedCodeRule(List<string> Allowed) : ITieredPolicyRuleRule
{
    public List<string> Check(TieredPolicyRule item)
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

public sealed record TieredPolicyRuleBatchSizeRangeRule(int Min, int Max) : ITieredPolicyRuleRule
{
    public List<string> Check(TieredPolicyRule item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPolicyRuleVisibleGuardRule(int Limit) : ITieredPolicyRuleRule
{
    public List<string> Check(TieredPolicyRule item)
    {
        if (item.Visible && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyRuleSegmentLengthRule(int MaxLength) : ITieredPolicyRuleRule
{
    public List<string> Check(TieredPolicyRule item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyRuleRequiredSegmentRule(string Message) : ITieredPolicyRuleRule
{
    public List<string> Check(TieredPolicyRule item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy rule.</summary>
public sealed record TieredPolicyRuleValidator(List<ITieredPolicyRuleRule> Rules)
{
    public List<string> Validate(TieredPolicyRule item)
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

    public void RequireValid(TieredPolicyRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyRule> items)
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

/// <summary>Functions of tiered policy rule rules.</summary>
public static class TieredPolicyRuleRules
{
    public static TieredPolicyRuleValidator DefaultTieredPolicyRuleValidator()
    {
        return new TieredPolicyRuleValidator(
            new List<ITieredPolicyRuleRule>
            {
                new TieredPolicyRuleAllowedCodeRule(new List<string> { "lenient", "seasonal" }),
                new TieredPolicyRuleBatchSizeRangeRule(32, 3600),
                new TieredPolicyRuleVisibleGuardRule(120),
                new TieredPolicyRuleSegmentLengthRule(16),
                new TieredPolicyRuleRequiredSegmentRule("tiered policy rule needs this field"),
            });
    }
}
