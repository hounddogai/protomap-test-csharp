namespace Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>A rule a fallback floor plan request must pass. Check returns its problems, or none.</summary>
public interface IFallbackFloorPlanRequestRule
{
    List<string> Check(FallbackFloorPlanRequest item);
}

public sealed record FallbackFloorPlanRequestTitleLengthRule(int MaxLength) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanRequestCodeLengthRule(int MaxLength) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanRequestWeightRangeRule(double Min, double Max) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanRequestRequiredLabelRule(string Message) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanRequestRequiredTitleRule(string Message) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanRequestLeaveDaysRangeRule(int Min, int Max) : IFallbackFloorPlanRequestRule
{
    public List<string> Check(FallbackFloorPlanRequest item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback floor plan request.</summary>
public sealed record FallbackFloorPlanRequestValidator(List<IFallbackFloorPlanRequestRule> Rules)
{
    public List<string> Validate(FallbackFloorPlanRequest item)
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

    public void RequireValid(FallbackFloorPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackFloorPlanRequest> items)
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

/// <summary>Functions of fallback floor plan request rules.</summary>
public static class FallbackFloorPlanRequestRules
{
    public static FallbackFloorPlanRequestValidator DefaultFallbackFloorPlanRequestValidator()
    {
        return new FallbackFloorPlanRequestValidator(
            new List<IFallbackFloorPlanRequestRule>
            {
                new FallbackFloorPlanRequestTitleLengthRule(255),
                new FallbackFloorPlanRequestCodeLengthRule(255),
                new FallbackFloorPlanRequestWeightRangeRule(3.0, 3354.1),
                new FallbackFloorPlanRequestRequiredLabelRule("fallback floor plan request needs this field"),
                new FallbackFloorPlanRequestRequiredTitleRule("fallback floor plan request needs an entry"),
                new FallbackFloorPlanRequestLeaveDaysRangeRule(16, 3600),
            });
    }
}
