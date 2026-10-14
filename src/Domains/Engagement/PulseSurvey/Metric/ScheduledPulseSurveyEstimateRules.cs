namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pulse survey estimate must pass. Check returns its problems, or none.</summary>
public interface IScheduledPulseSurveyEstimateRule
{
    List<string> Check(ScheduledPulseSurveyEstimate item);
}

public sealed record ScheduledPulseSurveyEstimateSkillLengthRule(int MaxLength) : IScheduledPulseSurveyEstimateRule
{
    public List<string> Check(ScheduledPulseSurveyEstimate item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyEstimateRequiredSkillRule(string Message) : IScheduledPulseSurveyEstimateRule
{
    public List<string> Check(ScheduledPulseSurveyEstimate item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyEstimateAllowedSkillRule(List<string> Allowed) : IScheduledPulseSurveyEstimateRule
{
    public List<string> Check(ScheduledPulseSurveyEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPulseSurveyEstimateTitleLengthRule(int MaxLength) : IScheduledPulseSurveyEstimateRule
{
    public List<string> Check(ScheduledPulseSurveyEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pulse survey estimate.</summary>
public sealed record ScheduledPulseSurveyEstimateValidator(List<IScheduledPulseSurveyEstimateRule> Rules)
{
    public List<string> Validate(ScheduledPulseSurveyEstimate item)
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

    public void RequireValid(ScheduledPulseSurveyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPulseSurveyEstimate> items)
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

/// <summary>Functions of scheduled pulse survey estimate rules.</summary>
public static class ScheduledPulseSurveyEstimateRules
{
    public static ScheduledPulseSurveyEstimateValidator DefaultScheduledPulseSurveyEstimateValidator()
    {
        return new ScheduledPulseSurveyEstimateValidator(
            new List<IScheduledPulseSurveyEstimateRule>
            {
                new ScheduledPulseSurveyEstimateSkillLengthRule(16),
                new ScheduledPulseSurveyEstimateRequiredSkillRule("scheduled pulse survey estimate needs a value"),
                new ScheduledPulseSurveyEstimateAllowedSkillRule(new List<string> { "rolling", "rolling", "capped" }),
                new ScheduledPulseSurveyEstimateTitleLengthRule(1000),
            });
    }
}
