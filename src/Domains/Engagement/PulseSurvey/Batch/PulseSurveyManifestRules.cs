namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

/// <summary>A rule a pulse survey manifest must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyManifestRule
{
    List<string> Check(PulseSurveyManifest item);
}

public sealed record PulseSurveyManifestRequiredTitleRule(string Message) : IPulseSurveyManifestRule
{
    public List<string> Check(PulseSurveyManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyManifestCodeLengthRule(int MaxLength) : IPulseSurveyManifestRule
{
    public List<string> Check(PulseSurveyManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyManifestRequiredCodeRule(string Message) : IPulseSurveyManifestRule
{
    public List<string> Check(PulseSurveyManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey manifest.</summary>
public sealed record PulseSurveyManifestValidator(List<IPulseSurveyManifestRule> Rules)
{
    public List<string> Validate(PulseSurveyManifest item)
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

    public void RequireValid(PulseSurveyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyManifest> items)
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

/// <summary>Functions of pulse survey manifest rules.</summary>
public static class PulseSurveyManifestRules
{
    public static PulseSurveyManifestValidator DefaultPulseSurveyManifestValidator()
    {
        return new PulseSurveyManifestValidator(
            new List<IPulseSurveyManifestRule>
            {
                new PulseSurveyManifestRequiredTitleRule("pulse survey manifest needs this field"),
                new PulseSurveyManifestCodeLengthRule(1000),
                new PulseSurveyManifestRequiredCodeRule("pulse survey manifest needs this field"),
            });
    }
}
