namespace Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>A rule a fallback action plan request must pass. Check returns its problems, or none.</summary>
public interface IFallbackActionPlanRequestRule
{
    List<string> Check(FallbackActionPlanRequest item);
}

public sealed record FallbackActionPlanRequestGradeLengthRule(int MaxLength) : IFallbackActionPlanRequestRule
{
    public List<string> Check(FallbackActionPlanRequest item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanRequestRequiredGradeRule(string Message) : IFallbackActionPlanRequestRule
{
    public List<string> Check(FallbackActionPlanRequest item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanRequestGraceDaysRangeRule(int Min, int Max) : IFallbackActionPlanRequestRule
{
    public List<string> Check(FallbackActionPlanRequest item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackActionPlanRequestCodeLengthRule(int MaxLength) : IFallbackActionPlanRequestRule
{
    public List<string> Check(FallbackActionPlanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback action plan request.</summary>
public sealed record FallbackActionPlanRequestValidator(List<IFallbackActionPlanRequestRule> Rules)
{
    public List<string> Validate(FallbackActionPlanRequest item)
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

    public void RequireValid(FallbackActionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackActionPlanRequest> items)
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

/// <summary>Functions of fallback action plan request rules.</summary>
public static class FallbackActionPlanRequestRules
{
    public static FallbackActionPlanRequestValidator DefaultFallbackActionPlanRequestValidator()
    {
        return new FallbackActionPlanRequestValidator(
            new List<IFallbackActionPlanRequestRule>
            {
                new FallbackActionPlanRequestGradeLengthRule(32),
                new FallbackActionPlanRequestRequiredGradeRule("fallback action plan request needs a value"),
                new FallbackActionPlanRequestGraceDaysRangeRule(120, 255),
                new FallbackActionPlanRequestCodeLengthRule(120),
            });
    }
}
