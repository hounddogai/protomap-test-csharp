namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual pulse survey tier must pass. Check returns its problems, or none.</summary>
public interface IManualPulseSurveyTierRule
{
    List<string> Check(ManualPulseSurveyTier item);
}

public sealed record ManualPulseSurveyTierAllowedRegionRule(List<string> Allowed) : IManualPulseSurveyTierRule
{
    public List<string> Check(ManualPulseSurveyTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyTierCodeLengthRule(int MaxLength) : IManualPulseSurveyTierRule
{
    public List<string> Check(ManualPulseSurveyTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyTierTitleLengthRule(int MaxLength) : IManualPulseSurveyTierRule
{
    public List<string> Check(ManualPulseSurveyTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyTierRequiredCodeRule(string Message) : IManualPulseSurveyTierRule
{
    public List<string> Check(ManualPulseSurveyTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pulse survey tier.</summary>
public sealed record ManualPulseSurveyTierValidator(List<IManualPulseSurveyTierRule> Rules)
{
    public List<string> Validate(ManualPulseSurveyTier item)
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

    public void RequireValid(ManualPulseSurveyTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPulseSurveyTier> items)
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

/// <summary>Functions of manual pulse survey tier rules.</summary>
public static class ManualPulseSurveyTierRules
{
    public static ManualPulseSurveyTierValidator DefaultManualPulseSurveyTierValidator()
    {
        return new ManualPulseSurveyTierValidator(
            new List<IManualPulseSurveyTierRule>
            {
                new ManualPulseSurveyTierAllowedRegionRule(new List<string> { "scheduled", "primary" }),
                new ManualPulseSurveyTierCodeLengthRule(1000),
                new ManualPulseSurveyTierTitleLengthRule(1000),
                new ManualPulseSurveyTierRequiredCodeRule("manual pulse survey tier needs this field"),
            });
    }
}
