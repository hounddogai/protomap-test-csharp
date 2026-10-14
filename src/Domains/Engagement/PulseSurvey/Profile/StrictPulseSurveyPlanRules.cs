namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pulse survey plan must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyPlanRule
{
    List<string> Check(StrictPulseSurveyPlan item);
}

public sealed record StrictPulseSurveyPlanQuantityRangeRule(int Min, int Max) : IStrictPulseSurveyPlanRule
{
    public List<string> Check(StrictPulseSurveyPlan item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPlanCodeLengthRule(int MaxLength) : IStrictPulseSurveyPlanRule
{
    public List<string> Check(StrictPulseSurveyPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPlanAllowedSkillRule(List<string> Allowed) : IStrictPulseSurveyPlanRule
{
    public List<string> Check(StrictPulseSurveyPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPlanRequiredCodeRule(string Message) : IStrictPulseSurveyPlanRule
{
    public List<string> Check(StrictPulseSurveyPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyPlanLabelLengthRule(int MaxLength) : IStrictPulseSurveyPlanRule
{
    public List<string> Check(StrictPulseSurveyPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey plan.</summary>
public sealed record StrictPulseSurveyPlanValidator(List<IStrictPulseSurveyPlanRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyPlan item)
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

    public void RequireValid(StrictPulseSurveyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyPlan> items)
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

/// <summary>Functions of strict pulse survey plan rules.</summary>
public static class StrictPulseSurveyPlanRules
{
    public static StrictPulseSurveyPlanValidator DefaultStrictPulseSurveyPlanValidator()
    {
        return new StrictPulseSurveyPlanValidator(
            new List<IStrictPulseSurveyPlanRule>
            {
                new StrictPulseSurveyPlanQuantityRangeRule(1000, 86400),
                new StrictPulseSurveyPlanCodeLengthRule(255),
                new StrictPulseSurveyPlanAllowedSkillRule(new List<string> { "tiered", "batched", "strict" }),
                new StrictPulseSurveyPlanRequiredCodeRule("strict pulse survey plan needs this field"),
                new StrictPulseSurveyPlanLabelLengthRule(80),
            });
    }
}
