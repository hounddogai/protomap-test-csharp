namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan rule must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanRuleRule
{
    List<string> Check(ReturnPlanRule item);
}

public sealed record ReturnPlanRuleRequiredCategoryRule(string Message) : IReturnPlanRuleRule
{
    public List<string> Check(ReturnPlanRule item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanRuleAllowedCodeRule(List<string> Allowed) : IReturnPlanRuleRule
{
    public List<string> Check(ReturnPlanRule item)
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

public sealed record ReturnPlanRuleChannelLengthRule(int MaxLength) : IReturnPlanRuleRule
{
    public List<string> Check(ReturnPlanRule item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan rule.</summary>
public sealed record ReturnPlanRuleValidator(List<IReturnPlanRuleRule> Rules)
{
    public List<string> Validate(ReturnPlanRule item)
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

    public void RequireValid(ReturnPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanRule> items)
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

/// <summary>Functions of return plan rule rules.</summary>
public static class ReturnPlanRuleRules
{
    public static ReturnPlanRuleValidator DefaultReturnPlanRuleValidator()
    {
        return new ReturnPlanRuleValidator(
            new List<IReturnPlanRuleRule>
            {
                new ReturnPlanRuleRequiredCategoryRule("return plan rule needs this field"),
                new ReturnPlanRuleAllowedCodeRule(new List<string>()),
                new ReturnPlanRuleChannelLengthRule(32),
            });
    }
}
