namespace Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>A rule a succession plan threshold must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanThresholdRule
{
    List<string> Check(SuccessionPlanThreshold item);
}

public sealed record SuccessionPlanThresholdCodeLengthRule(int MaxLength) : ISuccessionPlanThresholdRule
{
    public List<string> Check(SuccessionPlanThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanThresholdFteRangeRule(double Min, double Max) : ISuccessionPlanThresholdRule
{
    public List<string> Check(SuccessionPlanThreshold item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanThresholdRequiredCodeRule(string Message) : ISuccessionPlanThresholdRule
{
    public List<string> Check(SuccessionPlanThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanThresholdRequiredTitleRule(string Message) : ISuccessionPlanThresholdRule
{
    public List<string> Check(SuccessionPlanThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanThresholdCapacityRangeRule(int Min, int Max) : ISuccessionPlanThresholdRule
{
    public List<string> Check(SuccessionPlanThreshold item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a succession plan threshold.</summary>
public sealed record SuccessionPlanThresholdValidator(List<ISuccessionPlanThresholdRule> Rules)
{
    public List<string> Validate(SuccessionPlanThreshold item)
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

    public void RequireValid(SuccessionPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanThreshold> items)
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

/// <summary>Functions of succession plan threshold rules.</summary>
public static class SuccessionPlanThresholdRules
{
    public static SuccessionPlanThresholdValidator DefaultSuccessionPlanThresholdValidator()
    {
        return new SuccessionPlanThresholdValidator(
            new List<ISuccessionPlanThresholdRule>
            {
                new SuccessionPlanThresholdCodeLengthRule(32),
                new SuccessionPlanThresholdFteRangeRule(0.7, 1383.0),
                new SuccessionPlanThresholdRequiredCodeRule("succession plan threshold needs an entry"),
                new SuccessionPlanThresholdRequiredTitleRule("succession plan threshold needs a value"),
                new SuccessionPlanThresholdCapacityRangeRule(16, 255),
            });
    }
}
