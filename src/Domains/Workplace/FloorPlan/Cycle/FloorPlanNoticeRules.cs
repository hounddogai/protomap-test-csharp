namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>A rule a floor plan notice must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanNoticeRule
{
    List<string> Check(FloorPlanNotice item);
}

public sealed record FloorPlanNoticeCodeLengthRule(int MaxLength) : IFloorPlanNoticeRule
{
    public List<string> Check(FloorPlanNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanNoticeRequiredCodeRule(string Message) : IFloorPlanNoticeRule
{
    public List<string> Check(FloorPlanNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanNoticeRequiredLocaleRule(string Message) : IFloorPlanNoticeRule
{
    public List<string> Check(FloorPlanNotice item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan notice.</summary>
public sealed record FloorPlanNoticeValidator(List<IFloorPlanNoticeRule> Rules)
{
    public List<string> Validate(FloorPlanNotice item)
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

    public void RequireValid(FloorPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanNotice> items)
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

/// <summary>Functions of floor plan notice rules.</summary>
public static class FloorPlanNoticeRules
{
    public static FloorPlanNoticeValidator DefaultFloorPlanNoticeValidator()
    {
        return new FloorPlanNoticeValidator(
            new List<IFloorPlanNoticeRule>
            {
                new FloorPlanNoticeCodeLengthRule(255),
                new FloorPlanNoticeRequiredCodeRule("floor plan notice needs this field"),
                new FloorPlanNoticeRequiredLocaleRule("floor plan notice needs a value"),
            });
    }
}
