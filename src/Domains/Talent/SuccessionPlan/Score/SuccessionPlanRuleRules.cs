namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan rule must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanRuleRule
{
    List<string> Check(SuccessionPlanRule item);
}

public sealed record SuccessionPlanRuleLimitRangeRule(int Min, int Max) : ISuccessionPlanRuleRule
{
    public List<string> Check(SuccessionPlanRule item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanRuleRequiredCodeRule(string Message) : ISuccessionPlanRuleRule
{
    public List<string> Check(SuccessionPlanRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRuleAllowedNameRule(List<string> Allowed) : ISuccessionPlanRuleRule
{
    public List<string> Check(SuccessionPlanRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan rule.</summary>
public sealed record SuccessionPlanRuleValidator(List<ISuccessionPlanRuleRule> Rules)
{
    public List<string> Validate(SuccessionPlanRule item)
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

    public void RequireValid(SuccessionPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanRule> items)
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

/// <summary>Functions of succession plan rule rules.</summary>
public static class SuccessionPlanRuleRules
{
    public static SuccessionPlanRuleValidator DefaultSuccessionPlanRuleValidator()
    {
        return new SuccessionPlanRuleValidator(
            new List<ISuccessionPlanRuleRule>
            {
                new SuccessionPlanRuleLimitRangeRule(64, 64),
                new SuccessionPlanRuleRequiredCodeRule("succession plan rule needs this field"),
                new SuccessionPlanRuleAllowedNameRule(new List<string> { "primary", "lenient", "regional" }),
            });
    }
}
