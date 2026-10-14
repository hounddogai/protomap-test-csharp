namespace Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>A rule a scheduled deduction cycle must pass. Check returns its problems, or none.</summary>
public interface IScheduledDeductionCycleRule
{
    List<string> Check(ScheduledDeductionCycle item);
}

public sealed record ScheduledDeductionCycleManagerIdLengthRule(int MaxLength) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeductionCycleRequiredUnitRule(string Message) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeductionCycleFteRangeRule(double Min, double Max) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
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

public sealed record ScheduledDeductionCycleCodeLengthRule(int MaxLength) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeductionCycleRateRangeRule(double Min, double Max) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledDeductionCycleUnitLengthRule(int MaxLength) : IScheduledDeductionCycleRule
{
    public List<string> Check(ScheduledDeductionCycle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled deduction cycle.</summary>
public sealed record ScheduledDeductionCycleValidator(List<IScheduledDeductionCycleRule> Rules)
{
    public List<string> Validate(ScheduledDeductionCycle item)
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

    public void RequireValid(ScheduledDeductionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledDeductionCycle> items)
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

/// <summary>Functions of scheduled deduction cycle rules.</summary>
public static class ScheduledDeductionCycleRules
{
    public static ScheduledDeductionCycleValidator DefaultScheduledDeductionCycleValidator()
    {
        return new ScheduledDeductionCycleValidator(
            new List<IScheduledDeductionCycleRule>
            {
                new ScheduledDeductionCycleManagerIdLengthRule(1000),
                new ScheduledDeductionCycleRequiredUnitRule("scheduled deduction cycle needs a value"),
                new ScheduledDeductionCycleFteRangeRule(7.4, 4708.8),
                new ScheduledDeductionCycleCodeLengthRule(80),
                new ScheduledDeductionCycleRateRangeRule(0.6, 2440.0),
                new ScheduledDeductionCycleUnitLengthRule(86400),
            });
    }
}
