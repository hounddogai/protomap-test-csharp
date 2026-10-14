namespace Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>A rule a return plan cycle must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanCycleRule
{
    List<string> Check(ReturnPlanCycle item);
}

public sealed record ReturnPlanCyclePriorityRangeRule(int Min, int Max) : IReturnPlanCycleRule
{
    public List<string> Check(ReturnPlanCycle item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCycleRequiredTitleRule(string Message) : IReturnPlanCycleRule
{
    public List<string> Check(ReturnPlanCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCycleGradeLengthRule(int MaxLength) : IReturnPlanCycleRule
{
    public List<string> Check(ReturnPlanCycle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanCycleActiveGuardRule(int Limit) : IReturnPlanCycleRule
{
    public List<string> Check(ReturnPlanCycle item)
    {
        if (item.Active && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan cycle.</summary>
public sealed record ReturnPlanCycleValidator(List<IReturnPlanCycleRule> Rules)
{
    public List<string> Validate(ReturnPlanCycle item)
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

    public void RequireValid(ReturnPlanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanCycle> items)
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

/// <summary>Functions of return plan cycle rules.</summary>
public static class ReturnPlanCycleRules
{
    public static ReturnPlanCycleValidator DefaultReturnPlanCycleValidator()
    {
        return new ReturnPlanCycleValidator(
            new List<IReturnPlanCycleRule>
            {
                new ReturnPlanCyclePriorityRangeRule(32, 86400),
                new ReturnPlanCycleRequiredTitleRule("return plan cycle needs a value"),
                new ReturnPlanCycleGradeLengthRule(120),
                new ReturnPlanCycleActiveGuardRule(86400),
            });
    }
}
