namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pulse survey template must pass. Check returns its problems, or none.</summary>
public interface IRollingPulseSurveyTemplateRule
{
    List<string> Check(RollingPulseSurveyTemplate item);
}

public sealed record RollingPulseSurveyTemplateDiscountRangeRule(double Min, double Max) : IRollingPulseSurveyTemplateRule
{
    public List<string> Check(RollingPulseSurveyTemplate item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingPulseSurveyTemplateAllowedCodeRule(List<string> Allowed) : IRollingPulseSurveyTemplateRule
{
    public List<string> Check(RollingPulseSurveyTemplate item)
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

public sealed record RollingPulseSurveyTemplateCodePrefixRule(string Prefix) : IRollingPulseSurveyTemplateRule
{
    public List<string> Check(RollingPulseSurveyTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pulse survey template.</summary>
public sealed record RollingPulseSurveyTemplateValidator(List<IRollingPulseSurveyTemplateRule> Rules)
{
    public List<string> Validate(RollingPulseSurveyTemplate item)
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

    public void RequireValid(RollingPulseSurveyTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPulseSurveyTemplate> items)
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

/// <summary>Functions of rolling pulse survey template rules.</summary>
public static class RollingPulseSurveyTemplateRules
{
    public static RollingPulseSurveyTemplateValidator DefaultRollingPulseSurveyTemplateValidator()
    {
        return new RollingPulseSurveyTemplateValidator(
            new List<IRollingPulseSurveyTemplateRule>
            {
                new RollingPulseSurveyTemplateDiscountRangeRule(1.5, 1445.1),
                new RollingPulseSurveyTemplateAllowedCodeRule(new List<string> { "manual", "fallback", "pending" }),
                new RollingPulseSurveyTemplateCodePrefixRule("Q"),
            });
    }
}
