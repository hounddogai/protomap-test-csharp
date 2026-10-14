namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>A rule a default position adjustment must pass. Check returns its problems, or none.</summary>
public interface IDefaultPositionAdjustmentRule
{
    List<string> Check(DefaultPositionAdjustment item);
}

public sealed record DefaultPositionAdjustmentWindowDaysRangeRule(int Min, int Max) : IDefaultPositionAdjustmentRule
{
    public List<string> Check(DefaultPositionAdjustment item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPositionAdjustmentRequiredJobCodeRule(string Message) : IDefaultPositionAdjustmentRule
{
    public List<string> Check(DefaultPositionAdjustment item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPositionAdjustmentRequiredTitleRule(string Message) : IDefaultPositionAdjustmentRule
{
    public List<string> Check(DefaultPositionAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default position adjustment.</summary>
public sealed record DefaultPositionAdjustmentValidator(List<IDefaultPositionAdjustmentRule> Rules)
{
    public List<string> Validate(DefaultPositionAdjustment item)
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

    public void RequireValid(DefaultPositionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPositionAdjustment> items)
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

/// <summary>Functions of default position adjustment rules.</summary>
public static class DefaultPositionAdjustmentRules
{
    public static DefaultPositionAdjustmentValidator DefaultDefaultPositionAdjustmentValidator()
    {
        return new DefaultPositionAdjustmentValidator(
            new List<IDefaultPositionAdjustmentRule>
            {
                new DefaultPositionAdjustmentWindowDaysRangeRule(16, 86400),
                new DefaultPositionAdjustmentRequiredJobCodeRule("default position adjustment needs a value"),
                new DefaultPositionAdjustmentRequiredTitleRule("default position adjustment needs a value"),
            });
    }
}
