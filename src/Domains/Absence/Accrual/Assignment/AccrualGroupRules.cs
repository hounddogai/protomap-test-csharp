namespace Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>A rule a accrual group must pass. Check returns its problems, or none.</summary>
public interface IAccrualGroupRule
{
    List<string> Check(AccrualGroup item);
}

public sealed record AccrualGroupCodeLengthRule(int MaxLength) : IAccrualGroupRule
{
    public List<string> Check(AccrualGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualGroupRequiredEmployeeIdRule(string Message) : IAccrualGroupRule
{
    public List<string> Check(AccrualGroup item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualGroupBatchSizeRangeRule(int Min, int Max) : IAccrualGroupRule
{
    public List<string> Check(AccrualGroup item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualGroupEmployeeIdLengthRule(int MaxLength) : IAccrualGroupRule
{
    public List<string> Check(AccrualGroup item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual group.</summary>
public sealed record AccrualGroupValidator(List<IAccrualGroupRule> Rules)
{
    public List<string> Validate(AccrualGroup item)
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

    public void RequireValid(AccrualGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualGroup> items)
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

/// <summary>Functions of accrual group rules.</summary>
public static class AccrualGroupRules
{
    public static AccrualGroupValidator DefaultAccrualGroupValidator()
    {
        return new AccrualGroupValidator(
            new List<IAccrualGroupRule>
            {
                new AccrualGroupCodeLengthRule(80),
                new AccrualGroupRequiredEmployeeIdRule("accrual group needs a value"),
                new AccrualGroupBatchSizeRangeRule(80, 80),
                new AccrualGroupEmployeeIdLengthRule(80),
            });
    }
}
