namespace Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>A rule a pay run plan must pass. Check returns its problems, or none.</summary>
public interface IPayRunPlanRule
{
    List<string> Check(PayRunPlan item);
}

public sealed record PayRunPlanCodeLengthRule(int MaxLength) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPlanRateRangeRule(double Min, double Max) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
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

public sealed record PayRunPlanMaximumRangeRule(double Min, double Max) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPlanUnitLengthRule(int MaxLength) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPlanRequiredBadgeRule(string Message) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunPlanNameLengthRule(int MaxLength) : IPayRunPlanRule
{
    public List<string> Check(PayRunPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run plan.</summary>
public sealed record PayRunPlanValidator(List<IPayRunPlanRule> Rules)
{
    public List<string> Validate(PayRunPlan item)
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

    public void RequireValid(PayRunPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunPlan> items)
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

/// <summary>Functions of pay run plan rules.</summary>
public static class PayRunPlanRules
{
    public static PayRunPlanValidator DefaultPayRunPlanValidator()
    {
        return new PayRunPlanValidator(
            new List<IPayRunPlanRule>
            {
                new PayRunPlanCodeLengthRule(32),
                new PayRunPlanRateRangeRule(9.7, 3925.3),
                new PayRunPlanMaximumRangeRule(5.6, 4933.7),
                new PayRunPlanUnitLengthRule(80),
                new PayRunPlanRequiredBadgeRule("pay run plan needs an entry"),
                new PayRunPlanNameLengthRule(16),
            });
    }
}
