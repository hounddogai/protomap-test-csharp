namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>A rule a strict pulse survey preference must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyPreferenceRule
{
    List<string> Check(StrictPulseSurveyPreference item);
}

public sealed record StrictPulseSurveyPreferenceRequiredGradeRule(string Message) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPreferenceWindowDaysRangeRule(int Min, int Max) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPreferenceCodeLengthRule(int MaxLength) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPreferenceCountryLengthRule(int MaxLength) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPreferenceGradeLengthRule(int MaxLength) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPreferenceCodePrefixRule(string Prefix) : IStrictPulseSurveyPreferenceRule
{
    public List<string> Check(StrictPulseSurveyPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey preference.</summary>
public sealed record StrictPulseSurveyPreferenceValidator(List<IStrictPulseSurveyPreferenceRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyPreference item)
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

    public void RequireValid(StrictPulseSurveyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyPreference> items)
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

/// <summary>Functions of strict pulse survey preference rules.</summary>
public static class StrictPulseSurveyPreferenceRules
{
    public static StrictPulseSurveyPreferenceValidator DefaultStrictPulseSurveyPreferenceValidator()
    {
        return new StrictPulseSurveyPreferenceValidator(
            new List<IStrictPulseSurveyPreferenceRule>
            {
                new StrictPulseSurveyPreferenceRequiredGradeRule("strict pulse survey preference needs this field"),
                new StrictPulseSurveyPreferenceWindowDaysRangeRule(80, 86400),
                new StrictPulseSurveyPreferenceCodeLengthRule(86400),
                new StrictPulseSurveyPreferenceCountryLengthRule(80),
                new StrictPulseSurveyPreferenceGradeLengthRule(32),
                new StrictPulseSurveyPreferenceCodePrefixRule("REF"),
            });
    }
}
