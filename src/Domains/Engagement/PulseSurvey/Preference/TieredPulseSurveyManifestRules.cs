namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pulse survey manifest must pass. Check returns its problems, or none.</summary>
public interface ITieredPulseSurveyManifestRule
{
    List<string> Check(TieredPulseSurveyManifest item);
}

public sealed record TieredPulseSurveyManifestTitleLengthRule(int MaxLength) : ITieredPulseSurveyManifestRule
{
    public List<string> Check(TieredPulseSurveyManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPulseSurveyManifestCodePrefixRule(string Prefix) : ITieredPulseSurveyManifestRule
{
    public List<string> Check(TieredPulseSurveyManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPulseSurveyManifestAllowedTitleRule(List<string> Allowed) : ITieredPulseSurveyManifestRule
{
    public List<string> Check(TieredPulseSurveyManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TieredPulseSurveyManifestJobCodeLengthRule(int MaxLength) : ITieredPulseSurveyManifestRule
{
    public List<string> Check(TieredPulseSurveyManifest item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pulse survey manifest.</summary>
public sealed record TieredPulseSurveyManifestValidator(List<ITieredPulseSurveyManifestRule> Rules)
{
    public List<string> Validate(TieredPulseSurveyManifest item)
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

    public void RequireValid(TieredPulseSurveyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPulseSurveyManifest> items)
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

/// <summary>Functions of tiered pulse survey manifest rules.</summary>
public static class TieredPulseSurveyManifestRules
{
    public static TieredPulseSurveyManifestValidator DefaultTieredPulseSurveyManifestValidator()
    {
        return new TieredPulseSurveyManifestValidator(
            new List<ITieredPulseSurveyManifestRule>
            {
                new TieredPulseSurveyManifestTitleLengthRule(32),
                new TieredPulseSurveyManifestCodePrefixRule("X"),
                new TieredPulseSurveyManifestAllowedTitleRule(new List<string> { "pending", "rolling" }),
                new TieredPulseSurveyManifestJobCodeLengthRule(1000),
            });
    }
}
