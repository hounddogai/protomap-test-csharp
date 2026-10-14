namespace Acme.Workforce.Assets.Assignment.Quota;

/// <summary>A rule a assignment plan must pass. Check returns its problems, or none.</summary>
public interface IAssignmentPlanRule
{
    List<string> Check(AssignmentPlan item);
}

public sealed record AssignmentPlanScoreRangeRule(double Min, double Max) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentPlanRegionLengthRule(int MaxLength) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPlanRequiredRegionRule(string Message) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPlanActiveGuardRule(double Limit) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        if (item.Active && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPlanCodeLengthRule(int MaxLength) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPlanSummaryLengthRule(int MaxLength) : IAssignmentPlanRule
{
    public List<string> Check(AssignmentPlan item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment plan.</summary>
public sealed record AssignmentPlanValidator(List<IAssignmentPlanRule> Rules)
{
    public List<string> Validate(AssignmentPlan item)
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

    public void RequireValid(AssignmentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentPlan> items)
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

/// <summary>Functions of assignment plan rules.</summary>
public static class AssignmentPlanRules
{
    public static AssignmentPlanValidator DefaultAssignmentPlanValidator()
    {
        return new AssignmentPlanValidator(
            new List<IAssignmentPlanRule>
            {
                new AssignmentPlanScoreRangeRule(3.1, 3515.5),
                new AssignmentPlanRegionLengthRule(3600),
                new AssignmentPlanRequiredRegionRule("assignment plan needs a value"),
                new AssignmentPlanActiveGuardRule(9.8),
                new AssignmentPlanCodeLengthRule(86400),
                new AssignmentPlanSummaryLengthRule(80),
            });
    }
}
