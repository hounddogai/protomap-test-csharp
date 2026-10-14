namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>A rule a global pulse survey preference must pass. Check returns its problems, or none.</summary>
public interface IGlobalPulseSurveyPreferenceRule
{
    List<string> Check(GlobalPulseSurveyPreference item);
}

public sealed record GlobalPulseSurveyPreferenceTitleLengthRule(int MaxLength) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyPreferenceBudgetRangeRule(double Min, double Max) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyPreferenceRequiredCodeRule(string Message) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyPreferenceCodePrefixRule(string Prefix) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyPreferenceSourceLengthRule(int MaxLength) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyPreferencePinnedGuardRule(double Limit) : IGlobalPulseSurveyPreferenceRule
{
    public List<string> Check(GlobalPulseSurveyPreference item)
    {
        if (item.Pinned && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global pulse survey preference.</summary>
public sealed record GlobalPulseSurveyPreferenceValidator(List<IGlobalPulseSurveyPreferenceRule> Rules)
{
    public List<string> Validate(GlobalPulseSurveyPreference item)
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

    public void RequireValid(GlobalPulseSurveyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPulseSurveyPreference> items)
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

/// <summary>Functions of global pulse survey preference rules.</summary>
public static class GlobalPulseSurveyPreferenceRules
{
    public static GlobalPulseSurveyPreferenceValidator DefaultGlobalPulseSurveyPreferenceValidator()
    {
        return new GlobalPulseSurveyPreferenceValidator(
            new List<IGlobalPulseSurveyPreferenceRule>
            {
                new GlobalPulseSurveyPreferenceTitleLengthRule(86400),
                new GlobalPulseSurveyPreferenceBudgetRangeRule(1.7, 1302.3),
                new GlobalPulseSurveyPreferenceRequiredCodeRule("global pulse survey preference needs this field"),
                new GlobalPulseSurveyPreferenceCodePrefixRule("X"),
                new GlobalPulseSurveyPreferenceSourceLengthRule(86400),
                new GlobalPulseSurveyPreferencePinnedGuardRule(9.2),
            });
    }
}
