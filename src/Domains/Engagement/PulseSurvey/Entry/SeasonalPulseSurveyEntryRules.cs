namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>A rule a seasonal pulse survey entry must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPulseSurveyEntryRule
{
    List<string> Check(SeasonalPulseSurveyEntry item);
}

public sealed record SeasonalPulseSurveyEntryPriorityRangeRule(int Min, int Max) : ISeasonalPulseSurveyEntryRule
{
    public List<string> Check(SeasonalPulseSurveyEntry item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyEntryRequiredCategoryRule(string Message) : ISeasonalPulseSurveyEntryRule
{
    public List<string> Check(SeasonalPulseSurveyEntry item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyEntryCodePrefixRule(string Prefix) : ISeasonalPulseSurveyEntryRule
{
    public List<string> Check(SeasonalPulseSurveyEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pulse survey entry.</summary>
public sealed record SeasonalPulseSurveyEntryValidator(List<ISeasonalPulseSurveyEntryRule> Rules)
{
    public List<string> Validate(SeasonalPulseSurveyEntry item)
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

    public void RequireValid(SeasonalPulseSurveyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPulseSurveyEntry> items)
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

/// <summary>Functions of seasonal pulse survey entry rules.</summary>
public static class SeasonalPulseSurveyEntryRules
{
    public static SeasonalPulseSurveyEntryValidator DefaultSeasonalPulseSurveyEntryValidator()
    {
        return new SeasonalPulseSurveyEntryValidator(
            new List<ISeasonalPulseSurveyEntryRule>
            {
                new SeasonalPulseSurveyEntryPriorityRangeRule(255, 255),
                new SeasonalPulseSurveyEntryRequiredCategoryRule("seasonal pulse survey entry needs this field"),
                new SeasonalPulseSurveyEntryCodePrefixRule("Q"),
            });
    }
}
