namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global pulse survey adjustment must pass. Check returns its problems, or none.</summary>
public interface IGlobalPulseSurveyAdjustmentRule
{
    List<string> Check(GlobalPulseSurveyAdjustment item);
}

public sealed record GlobalPulseSurveyAdjustmentAllowedNameRule(List<string> Allowed) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyAdjustmentRequiredLabelRule(string Message) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyAdjustmentCodeLengthRule(int MaxLength) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyAdjustmentStartsAtRangeRule(int Min, int Max) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalPulseSurveyAdjustmentNameLengthRule(int MaxLength) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyAdjustmentEndsAtRangeRule(int Min, int Max) : IGlobalPulseSurveyAdjustmentRule
{
    public List<string> Check(GlobalPulseSurveyAdjustment item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global pulse survey adjustment.</summary>
public sealed record GlobalPulseSurveyAdjustmentValidator(List<IGlobalPulseSurveyAdjustmentRule> Rules)
{
    public List<string> Validate(GlobalPulseSurveyAdjustment item)
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

    public void RequireValid(GlobalPulseSurveyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPulseSurveyAdjustment> items)
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

/// <summary>Functions of global pulse survey adjustment rules.</summary>
public static class GlobalPulseSurveyAdjustmentRules
{
    public static GlobalPulseSurveyAdjustmentValidator DefaultGlobalPulseSurveyAdjustmentValidator()
    {
        return new GlobalPulseSurveyAdjustmentValidator(
            new List<IGlobalPulseSurveyAdjustmentRule>
            {
                new GlobalPulseSurveyAdjustmentAllowedNameRule(new List<string> { "pending", "lenient" }),
                new GlobalPulseSurveyAdjustmentRequiredLabelRule("global pulse survey adjustment needs an entry"),
                new GlobalPulseSurveyAdjustmentCodeLengthRule(1000),
                new GlobalPulseSurveyAdjustmentStartsAtRangeRule(16, 80),
                new GlobalPulseSurveyAdjustmentNameLengthRule(120),
                new GlobalPulseSurveyAdjustmentEndsAtRangeRule(1000, 1000),
            });
    }
}
