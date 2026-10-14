namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>A rule a manual retro pay budget must pass. Check returns its problems, or none.</summary>
public interface IManualRetroPayBudgetRule
{
    List<string> Check(ManualRetroPayBudget item);
}

public sealed record ManualRetroPayBudgetCodePrefixRule(string Prefix) : IManualRetroPayBudgetRule
{
    public List<string> Check(ManualRetroPayBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayBudgetRequiredCodeRule(string Message) : IManualRetroPayBudgetRule
{
    public List<string> Check(ManualRetroPayBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayBudgetEndsAtRangeRule(int Min, int Max) : IManualRetroPayBudgetRule
{
    public List<string> Check(ManualRetroPayBudget item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayBudgetRequiredLabelRule(string Message) : IManualRetroPayBudgetRule
{
    public List<string> Check(ManualRetroPayBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual retro pay budget.</summary>
public sealed record ManualRetroPayBudgetValidator(List<IManualRetroPayBudgetRule> Rules)
{
    public List<string> Validate(ManualRetroPayBudget item)
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

    public void RequireValid(ManualRetroPayBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRetroPayBudget> items)
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

/// <summary>Functions of manual retro pay budget rules.</summary>
public static class ManualRetroPayBudgetRules
{
    public static ManualRetroPayBudgetValidator DefaultManualRetroPayBudgetValidator()
    {
        return new ManualRetroPayBudgetValidator(
            new List<IManualRetroPayBudgetRule>
            {
                new ManualRetroPayBudgetCodePrefixRule("X"),
                new ManualRetroPayBudgetRequiredCodeRule("manual retro pay budget needs a value"),
                new ManualRetroPayBudgetEndsAtRangeRule(16, 255),
                new ManualRetroPayBudgetRequiredLabelRule("manual retro pay budget needs this field"),
            });
    }
}
