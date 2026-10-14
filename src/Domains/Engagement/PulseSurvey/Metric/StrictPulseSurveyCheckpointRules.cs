namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pulse survey checkpoint must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyCheckpointRule
{
    List<string> Check(StrictPulseSurveyCheckpoint item);
}

public sealed record StrictPulseSurveyCheckpointAllowedCodeRule(List<string> Allowed) : IStrictPulseSurveyCheckpointRule
{
    public List<string> Check(StrictPulseSurveyCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyCheckpointLabelLengthRule(int MaxLength) : IStrictPulseSurveyCheckpointRule
{
    public List<string> Check(StrictPulseSurveyCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyCheckpointCodeLengthRule(int MaxLength) : IStrictPulseSurveyCheckpointRule
{
    public List<string> Check(StrictPulseSurveyCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyCheckpointRequiredDescriptionRule(string Message) : IStrictPulseSurveyCheckpointRule
{
    public List<string> Check(StrictPulseSurveyCheckpoint item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyCheckpointPinnedGuardRule(int Limit) : IStrictPulseSurveyCheckpointRule
{
    public List<string> Check(StrictPulseSurveyCheckpoint item)
    {
        if (item.Pinned && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey checkpoint.</summary>
public sealed record StrictPulseSurveyCheckpointValidator(List<IStrictPulseSurveyCheckpointRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyCheckpoint item)
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

    public void RequireValid(StrictPulseSurveyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyCheckpoint> items)
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

/// <summary>Functions of strict pulse survey checkpoint rules.</summary>
public static class StrictPulseSurveyCheckpointRules
{
    public static StrictPulseSurveyCheckpointValidator DefaultStrictPulseSurveyCheckpointValidator()
    {
        return new StrictPulseSurveyCheckpointValidator(
            new List<IStrictPulseSurveyCheckpointRule>
            {
                new StrictPulseSurveyCheckpointAllowedCodeRule(new List<string> { "strict", "primary" }),
                new StrictPulseSurveyCheckpointLabelLengthRule(32),
                new StrictPulseSurveyCheckpointCodeLengthRule(1000),
                new StrictPulseSurveyCheckpointRequiredDescriptionRule(
                    "strict pulse survey checkpoint needs this field"),
                new StrictPulseSurveyCheckpointPinnedGuardRule(120),
            });
    }
}
