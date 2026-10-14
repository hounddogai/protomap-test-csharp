namespace Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>A rule a return plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanAdjustmentRule
{
    List<string> Check(ReturnPlanAdjustment item);
}

public sealed record ReturnPlanAdjustmentUpdatedAtRangeRule(int Min, int Max) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanAdjustmentWindowDaysRangeRule(int Min, int Max) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAdjustmentQuantityRangeRule(int Min, int Max) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanAdjustmentRequiredNameRule(string Message) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAdjustmentSequenceRangeRule(int Min, int Max) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAdjustmentNameLengthRule(int MaxLength) : IReturnPlanAdjustmentRule
{
    public List<string> Check(ReturnPlanAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan adjustment.</summary>
public sealed record ReturnPlanAdjustmentValidator(List<IReturnPlanAdjustmentRule> Rules)
{
    public List<string> Validate(ReturnPlanAdjustment item)
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

    public void RequireValid(ReturnPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanAdjustment> items)
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

/// <summary>Functions of return plan adjustment rules.</summary>
public static class ReturnPlanAdjustmentRules
{
    public static ReturnPlanAdjustmentValidator DefaultReturnPlanAdjustmentValidator()
    {
        return new ReturnPlanAdjustmentValidator(
            new List<IReturnPlanAdjustmentRule>
            {
                new ReturnPlanAdjustmentUpdatedAtRangeRule(80, 3600),
                new ReturnPlanAdjustmentWindowDaysRangeRule(120, 1000),
                new ReturnPlanAdjustmentQuantityRangeRule(120, 255),
                new ReturnPlanAdjustmentRequiredNameRule("return plan adjustment needs this field"),
                new ReturnPlanAdjustmentSequenceRangeRule(16, 1000),
                new ReturnPlanAdjustmentNameLengthRule(64),
            });
    }
}
