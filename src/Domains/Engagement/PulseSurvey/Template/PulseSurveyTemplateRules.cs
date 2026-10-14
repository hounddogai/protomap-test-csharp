namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey template must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyTemplateRule
{
    List<string> Check(PulseSurveyTemplate item);
}

public sealed record PulseSurveyTemplateAllowedLabelRule(List<string> Allowed) : IPulseSurveyTemplateRule
{
    public List<string> Check(PulseSurveyTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTemplateRequiredCategoryRule(string Message) : IPulseSurveyTemplateRule
{
    public List<string> Check(PulseSurveyTemplate item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTemplateCodePrefixRule(string Prefix) : IPulseSurveyTemplateRule
{
    public List<string> Check(PulseSurveyTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyTemplateRequiredCodeRule(string Message) : IPulseSurveyTemplateRule
{
    public List<string> Check(PulseSurveyTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey template.</summary>
public sealed record PulseSurveyTemplateValidator(List<IPulseSurveyTemplateRule> Rules)
{
    public List<string> Validate(PulseSurveyTemplate item)
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

    public void RequireValid(PulseSurveyTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyTemplate> items)
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

/// <summary>Functions of pulse survey template rules.</summary>
public static class PulseSurveyTemplateRules
{
    public static PulseSurveyTemplateValidator DefaultPulseSurveyTemplateValidator()
    {
        return new PulseSurveyTemplateValidator(
            new List<IPulseSurveyTemplateRule>
            {
                new PulseSurveyTemplateAllowedLabelRule(new List<string> { "rolling", "adaptive", "tiered" }),
                new PulseSurveyTemplateRequiredCategoryRule("pulse survey template needs a value"),
                new PulseSurveyTemplateCodePrefixRule("OPS"),
                new PulseSurveyTemplateRequiredCodeRule("pulse survey template needs an entry"),
            });
    }
}
