namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>A rule a seasonal pulse survey quota must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPulseSurveyQuotaRule
{
    List<string> Check(SeasonalPulseSurveyQuota item);
}

public sealed record SeasonalPulseSurveyQuotaCodeLengthRule(int MaxLength) : ISeasonalPulseSurveyQuotaRule
{
    public List<string> Check(SeasonalPulseSurveyQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyQuotaRequiredCodeRule(string Message) : ISeasonalPulseSurveyQuotaRule
{
    public List<string> Check(SeasonalPulseSurveyQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyQuotaExternalRefLengthRule(int MaxLength) : ISeasonalPulseSurveyQuotaRule
{
    public List<string> Check(SeasonalPulseSurveyQuota item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyQuotaCodePrefixRule(string Prefix) : ISeasonalPulseSurveyQuotaRule
{
    public List<string> Check(SeasonalPulseSurveyQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pulse survey quota.</summary>
public sealed record SeasonalPulseSurveyQuotaValidator(List<ISeasonalPulseSurveyQuotaRule> Rules)
{
    public List<string> Validate(SeasonalPulseSurveyQuota item)
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

    public void RequireValid(SeasonalPulseSurveyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPulseSurveyQuota> items)
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

/// <summary>Functions of seasonal pulse survey quota rules.</summary>
public static class SeasonalPulseSurveyQuotaRules
{
    public static SeasonalPulseSurveyQuotaValidator DefaultSeasonalPulseSurveyQuotaValidator()
    {
        return new SeasonalPulseSurveyQuotaValidator(
            new List<ISeasonalPulseSurveyQuotaRule>
            {
                new SeasonalPulseSurveyQuotaCodeLengthRule(16),
                new SeasonalPulseSurveyQuotaRequiredCodeRule("seasonal pulse survey quota needs this field"),
                new SeasonalPulseSurveyQuotaExternalRefLengthRule(255),
                new SeasonalPulseSurveyQuotaCodePrefixRule("X"),
            });
    }
}
