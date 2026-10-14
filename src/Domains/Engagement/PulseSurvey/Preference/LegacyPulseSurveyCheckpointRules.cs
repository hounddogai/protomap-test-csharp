namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>A rule a legacy pulse survey checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILegacyPulseSurveyCheckpointRule
{
    List<string> Check(LegacyPulseSurveyCheckpoint item);
}

public sealed record LegacyPulseSurveyCheckpointRequiredLabelRule(string Message) : ILegacyPulseSurveyCheckpointRule
{
    public List<string> Check(LegacyPulseSurveyCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPulseSurveyCheckpointManagerIdLengthRule(int MaxLength) : ILegacyPulseSurveyCheckpointRule
{
    public List<string> Check(LegacyPulseSurveyCheckpoint item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPulseSurveyCheckpointCodePrefixRule(string Prefix) : ILegacyPulseSurveyCheckpointRule
{
    public List<string> Check(LegacyPulseSurveyCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPulseSurveyCheckpointLabelLengthRule(int MaxLength) : ILegacyPulseSurveyCheckpointRule
{
    public List<string> Check(LegacyPulseSurveyCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPulseSurveyCheckpointRequiredCategoryRule(string Message) : ILegacyPulseSurveyCheckpointRule
{
    public List<string> Check(LegacyPulseSurveyCheckpoint item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pulse survey checkpoint.</summary>
public sealed record LegacyPulseSurveyCheckpointValidator(List<ILegacyPulseSurveyCheckpointRule> Rules)
{
    public List<string> Validate(LegacyPulseSurveyCheckpoint item)
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

    public void RequireValid(LegacyPulseSurveyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPulseSurveyCheckpoint> items)
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

/// <summary>Functions of legacy pulse survey checkpoint rules.</summary>
public static class LegacyPulseSurveyCheckpointRules
{
    public static LegacyPulseSurveyCheckpointValidator DefaultLegacyPulseSurveyCheckpointValidator()
    {
        return new LegacyPulseSurveyCheckpointValidator(
            new List<ILegacyPulseSurveyCheckpointRule>
            {
                new LegacyPulseSurveyCheckpointRequiredLabelRule("legacy pulse survey checkpoint needs a value"),
                new LegacyPulseSurveyCheckpointManagerIdLengthRule(32),
                new LegacyPulseSurveyCheckpointCodePrefixRule("OPS"),
                new LegacyPulseSurveyCheckpointLabelLengthRule(16),
                new LegacyPulseSurveyCheckpointRequiredCategoryRule("legacy pulse survey checkpoint needs a value"),
            });
    }
}
