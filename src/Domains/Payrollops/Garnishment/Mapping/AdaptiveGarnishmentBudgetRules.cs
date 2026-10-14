namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>A rule a adaptive garnishment budget must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveGarnishmentBudgetRule
{
    List<string> Check(AdaptiveGarnishmentBudget item);
}

public sealed record AdaptiveGarnishmentBudgetCodeLengthRule(int MaxLength) : IAdaptiveGarnishmentBudgetRule
{
    public List<string> Check(AdaptiveGarnishmentBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentBudgetRequiredCountryRule(string Message) : IAdaptiveGarnishmentBudgetRule
{
    public List<string> Check(AdaptiveGarnishmentBudget item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentBudgetCodePrefixRule(string Prefix) : IAdaptiveGarnishmentBudgetRule
{
    public List<string> Check(AdaptiveGarnishmentBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentBudgetRequiredReviewerRule(string Message) : IAdaptiveGarnishmentBudgetRule
{
    public List<string> Check(AdaptiveGarnishmentBudget item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentBudgetRateRangeRule(double Min, double Max) : IAdaptiveGarnishmentBudgetRule
{
    public List<string> Check(AdaptiveGarnishmentBudget item)
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

/// <summary>Runs every rule of a adaptive garnishment budget.</summary>
public sealed record AdaptiveGarnishmentBudgetValidator(List<IAdaptiveGarnishmentBudgetRule> Rules)
{
    public List<string> Validate(AdaptiveGarnishmentBudget item)
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

    public void RequireValid(AdaptiveGarnishmentBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveGarnishmentBudget> items)
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

/// <summary>Functions of adaptive garnishment budget rules.</summary>
public static class AdaptiveGarnishmentBudgetRules
{
    public static AdaptiveGarnishmentBudgetValidator DefaultAdaptiveGarnishmentBudgetValidator()
    {
        return new AdaptiveGarnishmentBudgetValidator(
            new List<IAdaptiveGarnishmentBudgetRule>
            {
                new AdaptiveGarnishmentBudgetCodeLengthRule(1000),
                new AdaptiveGarnishmentBudgetRequiredCountryRule("adaptive garnishment budget needs an entry"),
                new AdaptiveGarnishmentBudgetCodePrefixRule("Q"),
                new AdaptiveGarnishmentBudgetRequiredReviewerRule("adaptive garnishment budget needs an entry"),
                new AdaptiveGarnishmentBudgetRateRangeRule(4.5, 3513.5),
            });
    }
}
