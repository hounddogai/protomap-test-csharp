namespace Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>A rule a default floor plan target must pass. Check returns its problems, or none.</summary>
public interface IDefaultFloorPlanTargetRule
{
    List<string> Check(DefaultFloorPlanTarget item);
}

public sealed record DefaultFloorPlanTargetReviewerLengthRule(int MaxLength) : IDefaultFloorPlanTargetRule
{
    public List<string> Check(DefaultFloorPlanTarget item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanTargetRequiredCodeRule(string Message) : IDefaultFloorPlanTargetRule
{
    public List<string> Check(DefaultFloorPlanTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanTargetSequenceRangeRule(int Min, int Max) : IDefaultFloorPlanTargetRule
{
    public List<string> Check(DefaultFloorPlanTarget item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanTargetRequiredNameRule(string Message) : IDefaultFloorPlanTargetRule
{
    public List<string> Check(DefaultFloorPlanTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default floor plan target.</summary>
public sealed record DefaultFloorPlanTargetValidator(List<IDefaultFloorPlanTargetRule> Rules)
{
    public List<string> Validate(DefaultFloorPlanTarget item)
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

    public void RequireValid(DefaultFloorPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultFloorPlanTarget> items)
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

/// <summary>Functions of default floor plan target rules.</summary>
public static class DefaultFloorPlanTargetRules
{
    public static DefaultFloorPlanTargetValidator DefaultDefaultFloorPlanTargetValidator()
    {
        return new DefaultFloorPlanTargetValidator(
            new List<IDefaultFloorPlanTargetRule>
            {
                new DefaultFloorPlanTargetReviewerLengthRule(120),
                new DefaultFloorPlanTargetRequiredCodeRule("default floor plan target needs this field"),
                new DefaultFloorPlanTargetSequenceRangeRule(32, 32),
                new DefaultFloorPlanTargetRequiredNameRule("default floor plan target needs an entry"),
            });
    }
}
