namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan target must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanTargetRule
{
    List<string> Check(FloorPlanTarget item);
}

public sealed record FloorPlanTargetAllowedJobCodeRule(List<string> Allowed) : IFloorPlanTargetRule
{
    public List<string> Check(FloorPlanTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is too long" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTargetRequiredJobCodeRule(string Message) : IFloorPlanTargetRule
{
    public List<string> Check(FloorPlanTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTargetTitleLengthRule(int MaxLength) : IFloorPlanTargetRule
{
    public List<string> Check(FloorPlanTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTargetCodeLengthRule(int MaxLength) : IFloorPlanTargetRule
{
    public List<string> Check(FloorPlanTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTargetRequiredCodeRule(string Message) : IFloorPlanTargetRule
{
    public List<string> Check(FloorPlanTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan target.</summary>
public sealed record FloorPlanTargetValidator(List<IFloorPlanTargetRule> Rules)
{
    public List<string> Validate(FloorPlanTarget item)
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

    public void RequireValid(FloorPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanTarget> items)
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

/// <summary>Functions of floor plan target rules.</summary>
public static class FloorPlanTargetRules
{
    public static FloorPlanTargetValidator DefaultFloorPlanTargetValidator()
    {
        return new FloorPlanTargetValidator(
            new List<IFloorPlanTargetRule>
            {
                new FloorPlanTargetAllowedJobCodeRule(new List<string> { "fallback", "manual" }),
                new FloorPlanTargetRequiredJobCodeRule("floor plan target needs this field"),
                new FloorPlanTargetTitleLengthRule(16),
                new FloorPlanTargetCodeLengthRule(3600),
                new FloorPlanTargetRequiredCodeRule("floor plan target needs an entry"),
            });
    }
}
