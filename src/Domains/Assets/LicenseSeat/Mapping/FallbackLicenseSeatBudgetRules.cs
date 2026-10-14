namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>A rule a fallback license seat budget must pass. Check returns its problems, or none.</summary>
public interface IFallbackLicenseSeatBudgetRule
{
    List<string> Check(FallbackLicenseSeatBudget item);
}

public sealed record FallbackLicenseSeatBudgetCodePrefixRule(string Prefix) : IFallbackLicenseSeatBudgetRule
{
    public List<string> Check(FallbackLicenseSeatBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatBudgetRequiredTitleRule(string Message) : IFallbackLicenseSeatBudgetRule
{
    public List<string> Check(FallbackLicenseSeatBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatBudgetSalaryBandLengthRule(int MaxLength) : IFallbackLicenseSeatBudgetRule
{
    public List<string> Check(FallbackLicenseSeatBudget item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatBudgetRequiredCodeRule(string Message) : IFallbackLicenseSeatBudgetRule
{
    public List<string> Check(FallbackLicenseSeatBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback license seat budget.</summary>
public sealed record FallbackLicenseSeatBudgetValidator(List<IFallbackLicenseSeatBudgetRule> Rules)
{
    public List<string> Validate(FallbackLicenseSeatBudget item)
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

    public void RequireValid(FallbackLicenseSeatBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLicenseSeatBudget> items)
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

/// <summary>Functions of fallback license seat budget rules.</summary>
public static class FallbackLicenseSeatBudgetRules
{
    public static FallbackLicenseSeatBudgetValidator DefaultFallbackLicenseSeatBudgetValidator()
    {
        return new FallbackLicenseSeatBudgetValidator(
            new List<IFallbackLicenseSeatBudgetRule>
            {
                new FallbackLicenseSeatBudgetCodePrefixRule("X"),
                new FallbackLicenseSeatBudgetRequiredTitleRule("fallback license seat budget needs this field"),
                new FallbackLicenseSeatBudgetSalaryBandLengthRule(255),
                new FallbackLicenseSeatBudgetRequiredCodeRule("fallback license seat budget needs an entry"),
            });
    }
}
