namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>A rule a strict action plan threshold must pass. Check returns its problems, or none.</summary>
public interface IStrictActionPlanThresholdRule
{
    List<string> Check(StrictActionPlanThreshold item);
}

public sealed record StrictActionPlanThresholdSegmentLengthRule(int MaxLength) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictActionPlanThresholdRequiredSegmentRule(string Message) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictActionPlanThresholdNameLengthRule(int MaxLength) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictActionPlanThresholdRequiredCategoryRule(string Message) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictActionPlanThresholdRateRangeRule(double Min, double Max) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictActionPlanThresholdCodeLengthRule(int MaxLength) : IStrictActionPlanThresholdRule
{
    public List<string> Check(StrictActionPlanThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict action plan threshold.</summary>
public sealed record StrictActionPlanThresholdValidator(List<IStrictActionPlanThresholdRule> Rules)
{
    public List<string> Validate(StrictActionPlanThreshold item)
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

    public void RequireValid(StrictActionPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictActionPlanThreshold> items)
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

/// <summary>Functions of strict action plan threshold rules.</summary>
public static class StrictActionPlanThresholdRules
{
    public static StrictActionPlanThresholdValidator DefaultStrictActionPlanThresholdValidator()
    {
        return new StrictActionPlanThresholdValidator(
            new List<IStrictActionPlanThresholdRule>
            {
                new StrictActionPlanThresholdSegmentLengthRule(120),
                new StrictActionPlanThresholdRequiredSegmentRule("strict action plan threshold needs this field"),
                new StrictActionPlanThresholdNameLengthRule(3600),
                new StrictActionPlanThresholdRequiredCategoryRule("strict action plan threshold needs an entry"),
                new StrictActionPlanThresholdRateRangeRule(3.7, 1952.1),
                new StrictActionPlanThresholdCodeLengthRule(255),
            });
    }
}
