namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey override must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyOverrideRule
{
    List<string> Check(PulseSurveyOverride item);
}

public sealed record PulseSurveyOverrideLabelLengthRule(int MaxLength) : IPulseSurveyOverrideRule
{
    public List<string> Check(PulseSurveyOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyOverrideAllowedCategoryRule(List<string> Allowed) : IPulseSurveyOverrideRule
{
    public List<string> Check(PulseSurveyOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyOverrideUnitLengthRule(int MaxLength) : IPulseSurveyOverrideRule
{
    public List<string> Check(PulseSurveyOverride item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyOverrideRequiredCodeRule(string Message) : IPulseSurveyOverrideRule
{
    public List<string> Check(PulseSurveyOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey override.</summary>
public sealed record PulseSurveyOverrideValidator(List<IPulseSurveyOverrideRule> Rules)
{
    public List<string> Validate(PulseSurveyOverride item)
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

    public void RequireValid(PulseSurveyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyOverride> items)
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

/// <summary>Functions of pulse survey override rules.</summary>
public static class PulseSurveyOverrideRules
{
    public static PulseSurveyOverrideValidator DefaultPulseSurveyOverrideValidator()
    {
        return new PulseSurveyOverrideValidator(
            new List<IPulseSurveyOverrideRule>
            {
                new PulseSurveyOverrideLabelLengthRule(3600),
                new PulseSurveyOverrideAllowedCategoryRule(new List<string> { "global", "pending" }),
                new PulseSurveyOverrideUnitLengthRule(64),
                new PulseSurveyOverrideRequiredCodeRule("pulse survey override needs this field"),
            });
    }
}
