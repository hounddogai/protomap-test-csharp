namespace Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>A rule a deduction plan must pass. Check returns its problems, or none.</summary>
public interface IDeductionPlanRule
{
    List<string> Check(DeductionPlan item);
}

public sealed record DeductionPlanRequiredSegmentRule(string Message) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPlanPinnedGuardRule(double Limit) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        if (item.Pinned && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DeductionPlanRequiredDepartmentRule(string Message) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPlanLabelLengthRule(int MaxLength) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionPlanRequiredLabelRule(string Message) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPlanCeilingRangeRule(double Min, double Max) : IDeductionPlanRule
{
    public List<string> Check(DeductionPlan item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction plan.</summary>
public sealed record DeductionPlanValidator(List<IDeductionPlanRule> Rules)
{
    public List<string> Validate(DeductionPlan item)
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

    public void RequireValid(DeductionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionPlan> items)
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

/// <summary>Functions of deduction plan rules.</summary>
public static class DeductionPlanRules
{
    public static DeductionPlanValidator DefaultDeductionPlanValidator()
    {
        return new DeductionPlanValidator(
            new List<IDeductionPlanRule>
            {
                new DeductionPlanRequiredSegmentRule("deduction plan needs an entry"),
                new DeductionPlanPinnedGuardRule(7.2),
                new DeductionPlanRequiredDepartmentRule("deduction plan needs a value"),
                new DeductionPlanLabelLengthRule(32),
                new DeductionPlanRequiredLabelRule("deduction plan needs a value"),
                new DeductionPlanCeilingRangeRule(8.9, 345.8),
            });
    }
}
