namespace Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>A rule a default action plan policy must pass. Check returns its problems, or none.</summary>
public interface IDefaultActionPlanPolicyRule
{
    List<string> Check(DefaultActionPlanPolicy item);
}

public sealed record DefaultActionPlanPolicyDurationMinutesRangeRule(int Min, int Max) : IDefaultActionPlanPolicyRule
{
    public List<string> Check(DefaultActionPlanPolicy item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPolicyRequiredLabelRule(string Message) : IDefaultActionPlanPolicyRule
{
    public List<string> Check(DefaultActionPlanPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPolicyCodePrefixRule(string Prefix) : IDefaultActionPlanPolicyRule
{
    public List<string> Check(DefaultActionPlanPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPolicyRequiredCategoryRule(string Message) : IDefaultActionPlanPolicyRule
{
    public List<string> Check(DefaultActionPlanPolicy item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPolicyFteRangeRule(double Min, double Max) : IDefaultActionPlanPolicyRule
{
    public List<string> Check(DefaultActionPlanPolicy item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default action plan policy.</summary>
public sealed record DefaultActionPlanPolicyValidator(List<IDefaultActionPlanPolicyRule> Rules)
{
    public List<string> Validate(DefaultActionPlanPolicy item)
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

    public void RequireValid(DefaultActionPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultActionPlanPolicy> items)
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

/// <summary>Functions of default action plan policy rules.</summary>
public static class DefaultActionPlanPolicyRules
{
    public static DefaultActionPlanPolicyValidator DefaultDefaultActionPlanPolicyValidator()
    {
        return new DefaultActionPlanPolicyValidator(
            new List<IDefaultActionPlanPolicyRule>
            {
                new DefaultActionPlanPolicyDurationMinutesRangeRule(80, 120),
                new DefaultActionPlanPolicyRequiredLabelRule("default action plan policy needs a value"),
                new DefaultActionPlanPolicyCodePrefixRule("X"),
                new DefaultActionPlanPolicyRequiredCategoryRule("default action plan policy needs a value"),
                new DefaultActionPlanPolicyFteRangeRule(6.4, 3384.3),
            });
    }
}
