namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual pulse survey digest must pass. Check returns its problems, or none.</summary>
public interface IManualPulseSurveyDigestRule
{
    List<string> Check(ManualPulseSurveyDigest item);
}

public sealed record ManualPulseSurveyDigestAllowedNameRule(List<string> Allowed) : IManualPulseSurveyDigestRule
{
    public List<string> Check(ManualPulseSurveyDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyDigestTitleLengthRule(int MaxLength) : IManualPulseSurveyDigestRule
{
    public List<string> Check(ManualPulseSurveyDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyDigestRequiredNameRule(string Message) : IManualPulseSurveyDigestRule
{
    public List<string> Check(ManualPulseSurveyDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pulse survey digest.</summary>
public sealed record ManualPulseSurveyDigestValidator(List<IManualPulseSurveyDigestRule> Rules)
{
    public List<string> Validate(ManualPulseSurveyDigest item)
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

    public void RequireValid(ManualPulseSurveyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPulseSurveyDigest> items)
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

/// <summary>Functions of manual pulse survey digest rules.</summary>
public static class ManualPulseSurveyDigestRules
{
    public static ManualPulseSurveyDigestValidator DefaultManualPulseSurveyDigestValidator()
    {
        return new ManualPulseSurveyDigestValidator(
            new List<IManualPulseSurveyDigestRule>
            {
                new ManualPulseSurveyDigestAllowedNameRule(new List<string> { "manual", "primary", "default" }),
                new ManualPulseSurveyDigestTitleLengthRule(1000),
                new ManualPulseSurveyDigestRequiredNameRule("manual pulse survey digest needs this field"),
            });
    }
}
