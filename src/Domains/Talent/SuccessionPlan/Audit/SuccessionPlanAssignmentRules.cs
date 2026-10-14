namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>A rule a succession plan assignment must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanAssignmentRule
{
    List<string> Check(SuccessionPlanAssignment item);
}

public sealed record SuccessionPlanAssignmentRequiredSummaryRule(string Message) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAssignmentLocationLengthRule(int MaxLength) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAssignmentSummaryLengthRule(int MaxLength) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAssignmentCodeLengthRule(int MaxLength) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAssignmentBudgetRangeRule(double Min, double Max) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAssignmentFloorValueRangeRule(double Min, double Max) : ISuccessionPlanAssignmentRule
{
    public List<string> Check(SuccessionPlanAssignment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan assignment.</summary>
public sealed record SuccessionPlanAssignmentValidator(List<ISuccessionPlanAssignmentRule> Rules)
{
    public List<string> Validate(SuccessionPlanAssignment item)
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

    public void RequireValid(SuccessionPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanAssignment> items)
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

/// <summary>Functions of succession plan assignment rules.</summary>
public static class SuccessionPlanAssignmentRules
{
    public static SuccessionPlanAssignmentValidator DefaultSuccessionPlanAssignmentValidator()
    {
        return new SuccessionPlanAssignmentValidator(
            new List<ISuccessionPlanAssignmentRule>
            {
                new SuccessionPlanAssignmentRequiredSummaryRule("succession plan assignment needs this field"),
                new SuccessionPlanAssignmentLocationLengthRule(255),
                new SuccessionPlanAssignmentSummaryLengthRule(64),
                new SuccessionPlanAssignmentCodeLengthRule(86400),
                new SuccessionPlanAssignmentBudgetRangeRule(5.4, 1320.3),
                new SuccessionPlanAssignmentFloorValueRangeRule(9.1, 4220.6),
            });
    }
}
