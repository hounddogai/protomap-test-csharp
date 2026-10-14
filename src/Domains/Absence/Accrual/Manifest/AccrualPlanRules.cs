namespace Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>A rule a accrual plan must pass. Check returns its problems, or none.</summary>
public interface IAccrualPlanRule
{
    List<string> Check(AccrualPlan item);
}

public sealed record AccrualPlanRequiredTitleRule(string Message) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualPlanTitleLengthRule(int MaxLength) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPlanJobCodeLengthRule(int MaxLength) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPlanRequiredJobCodeRule(string Message) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualPlanPriorityRangeRule(int Min, int Max) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualPlanCodeLengthRule(int MaxLength) : IAccrualPlanRule
{
    public List<string> Check(AccrualPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual plan.</summary>
public sealed record AccrualPlanValidator(List<IAccrualPlanRule> Rules)
{
    public List<string> Validate(AccrualPlan item)
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

    public void RequireValid(AccrualPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualPlan> items)
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

/// <summary>Functions of accrual plan rules.</summary>
public static class AccrualPlanRules
{
    public static AccrualPlanValidator DefaultAccrualPlanValidator()
    {
        return new AccrualPlanValidator(
            new List<IAccrualPlanRule>
            {
                new AccrualPlanRequiredTitleRule("accrual plan needs a value"),
                new AccrualPlanTitleLengthRule(86400),
                new AccrualPlanJobCodeLengthRule(1000),
                new AccrualPlanRequiredJobCodeRule("accrual plan needs an entry"),
                new AccrualPlanPriorityRangeRule(1000, 86400),
                new AccrualPlanCodeLengthRule(86400),
            });
    }
}
