namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default succession plan variance must pass. Check returns its problems, or none.</summary>
public interface IDefaultSuccessionPlanVarianceRule
{
    List<string> Check(DefaultSuccessionPlanVariance item);
}

public sealed record DefaultSuccessionPlanVarianceRequiredDescriptionRule(string Message) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSuccessionPlanVarianceTitleLengthRule(int MaxLength) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSuccessionPlanVarianceThresholdRangeRule(double Min, double Max) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSuccessionPlanVarianceAllowedDescriptionRule(List<string> Allowed) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is missing" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSuccessionPlanVarianceRequiredTimezoneRule(string Message) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSuccessionPlanVarianceRequiredTitleRule(string Message) : IDefaultSuccessionPlanVarianceRule
{
    public List<string> Check(DefaultSuccessionPlanVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default succession plan variance.</summary>
public sealed record DefaultSuccessionPlanVarianceValidator(List<IDefaultSuccessionPlanVarianceRule> Rules)
{
    public List<string> Validate(DefaultSuccessionPlanVariance item)
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

    public void RequireValid(DefaultSuccessionPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSuccessionPlanVariance> items)
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

/// <summary>Functions of default succession plan variance rules.</summary>
public static class DefaultSuccessionPlanVarianceRules
{
    public static DefaultSuccessionPlanVarianceValidator DefaultDefaultSuccessionPlanVarianceValidator()
    {
        return new DefaultSuccessionPlanVarianceValidator(
            new List<IDefaultSuccessionPlanVarianceRule>
            {
                new DefaultSuccessionPlanVarianceRequiredDescriptionRule(
                    "default succession plan variance needs this field"),
                new DefaultSuccessionPlanVarianceTitleLengthRule(120),
                new DefaultSuccessionPlanVarianceThresholdRangeRule(8.8, 1791.9),
                new DefaultSuccessionPlanVarianceAllowedDescriptionRule(new List<string> { "lenient", "lenient" }),
                new DefaultSuccessionPlanVarianceRequiredTimezoneRule(
                    "default succession plan variance needs this field"),
                new DefaultSuccessionPlanVarianceRequiredTitleRule("default succession plan variance needs a value"),
            });
    }
}
