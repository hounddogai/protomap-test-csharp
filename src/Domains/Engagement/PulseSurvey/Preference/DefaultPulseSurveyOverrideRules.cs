namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>A rule a default pulse survey override must pass. Check returns its problems, or none.</summary>
public interface IDefaultPulseSurveyOverrideRule
{
    List<string> Check(DefaultPulseSurveyOverride item);
}

public sealed record DefaultPulseSurveyOverrideRequiredNameRule(string Message) : IDefaultPulseSurveyOverrideRule
{
    public List<string> Check(DefaultPulseSurveyOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPulseSurveyOverrideCodeLengthRule(int MaxLength) : IDefaultPulseSurveyOverrideRule
{
    public List<string> Check(DefaultPulseSurveyOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPulseSurveyOverrideCapacityRangeRule(int Min, int Max) : IDefaultPulseSurveyOverrideRule
{
    public List<string> Check(DefaultPulseSurveyOverride item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pulse survey override.</summary>
public sealed record DefaultPulseSurveyOverrideValidator(List<IDefaultPulseSurveyOverrideRule> Rules)
{
    public List<string> Validate(DefaultPulseSurveyOverride item)
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

    public void RequireValid(DefaultPulseSurveyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPulseSurveyOverride> items)
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

/// <summary>Functions of default pulse survey override rules.</summary>
public static class DefaultPulseSurveyOverrideRules
{
    public static DefaultPulseSurveyOverrideValidator DefaultDefaultPulseSurveyOverrideValidator()
    {
        return new DefaultPulseSurveyOverrideValidator(
            new List<IDefaultPulseSurveyOverrideRule>
            {
                new DefaultPulseSurveyOverrideRequiredNameRule("default pulse survey override needs this field"),
                new DefaultPulseSurveyOverrideCodeLengthRule(80),
                new DefaultPulseSurveyOverrideCapacityRangeRule(32, 32),
            });
    }
}
