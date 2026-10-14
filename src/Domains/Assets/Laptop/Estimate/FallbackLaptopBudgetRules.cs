namespace Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>A rule a fallback laptop budget must pass. Check returns its problems, or none.</summary>
public interface IFallbackLaptopBudgetRule
{
    List<string> Check(FallbackLaptopBudget item);
}

public sealed record FallbackLaptopBudgetRequiredExternalRefRule(string Message) : IFallbackLaptopBudgetRule
{
    public List<string> Check(FallbackLaptopBudget item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopBudgetCodePrefixRule(string Prefix) : IFallbackLaptopBudgetRule
{
    public List<string> Check(FallbackLaptopBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopBudgetRequiredCodeRule(string Message) : IFallbackLaptopBudgetRule
{
    public List<string> Check(FallbackLaptopBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopBudgetRequiredTitleRule(string Message) : IFallbackLaptopBudgetRule
{
    public List<string> Check(FallbackLaptopBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLaptopBudgetCodeLengthRule(int MaxLength) : IFallbackLaptopBudgetRule
{
    public List<string> Check(FallbackLaptopBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback laptop budget.</summary>
public sealed record FallbackLaptopBudgetValidator(List<IFallbackLaptopBudgetRule> Rules)
{
    public List<string> Validate(FallbackLaptopBudget item)
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

    public void RequireValid(FallbackLaptopBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLaptopBudget> items)
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

/// <summary>Functions of fallback laptop budget rules.</summary>
public static class FallbackLaptopBudgetRules
{
    public static FallbackLaptopBudgetValidator DefaultFallbackLaptopBudgetValidator()
    {
        return new FallbackLaptopBudgetValidator(
            new List<IFallbackLaptopBudgetRule>
            {
                new FallbackLaptopBudgetRequiredExternalRefRule("fallback laptop budget needs an entry"),
                new FallbackLaptopBudgetCodePrefixRule("Q"),
                new FallbackLaptopBudgetRequiredCodeRule("fallback laptop budget needs a value"),
                new FallbackLaptopBudgetRequiredTitleRule("fallback laptop budget needs this field"),
                new FallbackLaptopBudgetCodeLengthRule(32),
            });
    }
}
