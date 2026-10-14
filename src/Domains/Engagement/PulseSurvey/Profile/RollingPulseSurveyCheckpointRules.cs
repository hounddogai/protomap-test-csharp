namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pulse survey checkpoint must pass. Check returns its problems, or none.</summary>
public interface IRollingPulseSurveyCheckpointRule
{
    List<string> Check(RollingPulseSurveyCheckpoint item);
}

public sealed record RollingPulseSurveyCheckpointRequiredCodeRule(string Message) : IRollingPulseSurveyCheckpointRule
{
    public List<string> Check(RollingPulseSurveyCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPulseSurveyCheckpointRequiredSummaryRule(string Message) : IRollingPulseSurveyCheckpointRule
{
    public List<string> Check(RollingPulseSurveyCheckpoint item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPulseSurveyCheckpointAllowedNameRule(List<string> Allowed) : IRollingPulseSurveyCheckpointRule
{
    public List<string> Check(RollingPulseSurveyCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pulse survey checkpoint.</summary>
public sealed record RollingPulseSurveyCheckpointValidator(List<IRollingPulseSurveyCheckpointRule> Rules)
{
    public List<string> Validate(RollingPulseSurveyCheckpoint item)
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

    public void RequireValid(RollingPulseSurveyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPulseSurveyCheckpoint> items)
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

/// <summary>Functions of rolling pulse survey checkpoint rules.</summary>
public static class RollingPulseSurveyCheckpointRules
{
    public static RollingPulseSurveyCheckpointValidator DefaultRollingPulseSurveyCheckpointValidator()
    {
        return new RollingPulseSurveyCheckpointValidator(
            new List<IRollingPulseSurveyCheckpointRule>
            {
                new RollingPulseSurveyCheckpointRequiredCodeRule("rolling pulse survey checkpoint needs this field"),
                new RollingPulseSurveyCheckpointRequiredSummaryRule(
                    "rolling pulse survey checkpoint needs this field"),
                new RollingPulseSurveyCheckpointAllowedNameRule(
                    new List<string> { "seasonal", "manual", "regional" }),
            });
    }
}
