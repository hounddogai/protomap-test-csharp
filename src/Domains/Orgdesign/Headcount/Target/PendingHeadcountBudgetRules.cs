namespace Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>A rule a pending headcount budget must pass. Check returns its problems, or none.</summary>
public interface IPendingHeadcountBudgetRule
{
    List<string> Check(PendingHeadcountBudget item);
}

public sealed record PendingHeadcountBudgetCodePrefixRule(string Prefix) : IPendingHeadcountBudgetRule
{
    public List<string> Check(PendingHeadcountBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHeadcountBudgetRequiredJobCodeRule(string Message) : IPendingHeadcountBudgetRule
{
    public List<string> Check(PendingHeadcountBudget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingHeadcountBudgetCodeLengthRule(int MaxLength) : IPendingHeadcountBudgetRule
{
    public List<string> Check(PendingHeadcountBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHeadcountBudgetRateRangeRule(double Min, double Max) : IPendingHeadcountBudgetRule
{
    public List<string> Check(PendingHeadcountBudget item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending headcount budget.</summary>
public sealed record PendingHeadcountBudgetValidator(List<IPendingHeadcountBudgetRule> Rules)
{
    public List<string> Validate(PendingHeadcountBudget item)
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

    public void RequireValid(PendingHeadcountBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingHeadcountBudget> items)
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

/// <summary>Functions of pending headcount budget rules.</summary>
public static class PendingHeadcountBudgetRules
{
    public static PendingHeadcountBudgetValidator DefaultPendingHeadcountBudgetValidator()
    {
        return new PendingHeadcountBudgetValidator(
            new List<IPendingHeadcountBudgetRule>
            {
                new PendingHeadcountBudgetCodePrefixRule("ACM"),
                new PendingHeadcountBudgetRequiredJobCodeRule("pending headcount budget needs this field"),
                new PendingHeadcountBudgetCodeLengthRule(255),
                new PendingHeadcountBudgetRateRangeRule(4.5, 254.5),
            });
    }
}
