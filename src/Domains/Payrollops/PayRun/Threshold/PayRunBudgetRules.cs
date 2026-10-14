namespace Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>A rule a pay run budget must pass. Check returns its problems, or none.</summary>
public interface IPayRunBudgetRule
{
    List<string> Check(PayRunBudget item);
}

public sealed record PayRunBudgetNameLengthRule(int MaxLength) : IPayRunBudgetRule
{
    public List<string> Check(PayRunBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBudgetScoreRangeRule(double Min, double Max) : IPayRunBudgetRule
{
    public List<string> Check(PayRunBudget item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBudgetCodeLengthRule(int MaxLength) : IPayRunBudgetRule
{
    public List<string> Check(PayRunBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBudgetRequiredCodeRule(string Message) : IPayRunBudgetRule
{
    public List<string> Check(PayRunBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunBudgetRequiredNameRule(string Message) : IPayRunBudgetRule
{
    public List<string> Check(PayRunBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run budget.</summary>
public sealed record PayRunBudgetValidator(List<IPayRunBudgetRule> Rules)
{
    public List<string> Validate(PayRunBudget item)
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

    public void RequireValid(PayRunBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunBudget> items)
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

/// <summary>Functions of pay run budget rules.</summary>
public static class PayRunBudgetRules
{
    public static PayRunBudgetValidator DefaultPayRunBudgetValidator()
    {
        return new PayRunBudgetValidator(
            new List<IPayRunBudgetRule>
            {
                new PayRunBudgetNameLengthRule(255),
                new PayRunBudgetScoreRangeRule(9.0, 3977.1),
                new PayRunBudgetCodeLengthRule(3600),
                new PayRunBudgetRequiredCodeRule("pay run budget needs this field"),
                new PayRunBudgetRequiredNameRule("pay run budget needs this field"),
            });
    }
}
