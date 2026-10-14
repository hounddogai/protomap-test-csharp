namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pulse survey tier must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyTierRule
{
    List<string> Check(StrictPulseSurveyTier item);
}

public sealed record StrictPulseSurveyTierCodeLengthRule(int MaxLength) : IStrictPulseSurveyTierRule
{
    public List<string> Check(StrictPulseSurveyTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyTierCodePrefixRule(string Prefix) : IStrictPulseSurveyTierRule
{
    public List<string> Check(StrictPulseSurveyTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyTierAllowedLabelRule(List<string> Allowed) : IStrictPulseSurveyTierRule
{
    public List<string> Check(StrictPulseSurveyTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyTierRequiredLabelRule(string Message) : IStrictPulseSurveyTierRule
{
    public List<string> Check(StrictPulseSurveyTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyTierSummaryLengthRule(int MaxLength) : IStrictPulseSurveyTierRule
{
    public List<string> Check(StrictPulseSurveyTier item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey tier.</summary>
public sealed record StrictPulseSurveyTierValidator(List<IStrictPulseSurveyTierRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyTier item)
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

    public void RequireValid(StrictPulseSurveyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyTier> items)
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

/// <summary>Functions of strict pulse survey tier rules.</summary>
public static class StrictPulseSurveyTierRules
{
    public static StrictPulseSurveyTierValidator DefaultStrictPulseSurveyTierValidator()
    {
        return new StrictPulseSurveyTierValidator(
            new List<IStrictPulseSurveyTierRule>
            {
                new StrictPulseSurveyTierCodeLengthRule(3600),
                new StrictPulseSurveyTierCodePrefixRule("X"),
                new StrictPulseSurveyTierAllowedLabelRule(new List<string> { "manual", "tiered", "scheduled" }),
                new StrictPulseSurveyTierRequiredLabelRule("strict pulse survey tier needs an entry"),
                new StrictPulseSurveyTierSummaryLengthRule(64),
            });
    }
}
