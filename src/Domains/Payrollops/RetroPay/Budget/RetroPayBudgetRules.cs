namespace Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>A rule a retro pay budget must pass. Check returns its problems, or none.</summary>
public interface IRetroPayBudgetRule
{
    List<string> Check(RetroPayBudget item);
}

public sealed record RetroPayBudgetRequiredExternalRefRule(string Message) : IRetroPayBudgetRule
{
    public List<string> Check(RetroPayBudget item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBudgetVisibleGuardRule(double Limit) : IRetroPayBudgetRule
{
    public List<string> Check(RetroPayBudget item)
    {
        if (item.Visible && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBudgetCodeLengthRule(int MaxLength) : IRetroPayBudgetRule
{
    public List<string> Check(RetroPayBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay budget.</summary>
public sealed record RetroPayBudgetValidator(List<IRetroPayBudgetRule> Rules)
{
    public List<string> Validate(RetroPayBudget item)
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

    public void RequireValid(RetroPayBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayBudget> items)
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

/// <summary>Functions of retro pay budget rules.</summary>
public static class RetroPayBudgetRules
{
    public static RetroPayBudgetValidator DefaultRetroPayBudgetValidator()
    {
        return new RetroPayBudgetValidator(
            new List<IRetroPayBudgetRule>
            {
                new RetroPayBudgetRequiredExternalRefRule("retro pay budget needs this field"),
                new RetroPayBudgetVisibleGuardRule(6.5),
                new RetroPayBudgetCodeLengthRule(86400),
            });
    }
}
