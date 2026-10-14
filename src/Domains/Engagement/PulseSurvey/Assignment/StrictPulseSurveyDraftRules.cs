namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>A rule a strict pulse survey draft must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyDraftRule
{
    List<string> Check(StrictPulseSurveyDraft item);
}

public sealed record StrictPulseSurveyDraftTenureMonthsRangeRule(int Min, int Max) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictPulseSurveyDraftAttemptsRangeRule(int Min, int Max) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyDraftThresholdRangeRule(double Min, double Max) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyDraftRequiredCodeRule(string Message) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyDraftScoreRangeRule(double Min, double Max) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyDraftCodePrefixRule(string Prefix) : IStrictPulseSurveyDraftRule
{
    public List<string> Check(StrictPulseSurveyDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey draft.</summary>
public sealed record StrictPulseSurveyDraftValidator(List<IStrictPulseSurveyDraftRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyDraft item)
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

    public void RequireValid(StrictPulseSurveyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyDraft> items)
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

/// <summary>Functions of strict pulse survey draft rules.</summary>
public static class StrictPulseSurveyDraftRules
{
    public static StrictPulseSurveyDraftValidator DefaultStrictPulseSurveyDraftValidator()
    {
        return new StrictPulseSurveyDraftValidator(
            new List<IStrictPulseSurveyDraftRule>
            {
                new StrictPulseSurveyDraftTenureMonthsRangeRule(16, 64),
                new StrictPulseSurveyDraftAttemptsRangeRule(16, 3600),
                new StrictPulseSurveyDraftThresholdRangeRule(0.0, 1991.6),
                new StrictPulseSurveyDraftRequiredCodeRule("strict pulse survey draft needs an entry"),
                new StrictPulseSurveyDraftScoreRangeRule(4.6, 2213.0),
                new StrictPulseSurveyDraftCodePrefixRule("REF"),
            });
    }
}
