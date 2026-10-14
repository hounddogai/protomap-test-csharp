namespace Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>A rule a regional engagement budget must pass. Check returns its problems, or none.</summary>
public interface IRegionalEngagementBudgetRule
{
    List<string> Check(RegionalEngagementBudget item);
}

public sealed record RegionalEngagementBudgetWeightRangeRule(double Min, double Max) : IRegionalEngagementBudgetRule
{
    public List<string> Check(RegionalEngagementBudget item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementBudgetRequiredSalaryBandRule(string Message) : IRegionalEngagementBudgetRule
{
    public List<string> Check(RegionalEngagementBudget item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementBudgetExpiresAtRangeRule(int Min, int Max) : IRegionalEngagementBudgetRule
{
    public List<string> Check(RegionalEngagementBudget item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalEngagementBudgetCodePrefixRule(string Prefix) : IRegionalEngagementBudgetRule
{
    public List<string> Check(RegionalEngagementBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementBudgetRequiredLabelRule(string Message) : IRegionalEngagementBudgetRule
{
    public List<string> Check(RegionalEngagementBudget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional engagement budget.</summary>
public sealed record RegionalEngagementBudgetValidator(List<IRegionalEngagementBudgetRule> Rules)
{
    public List<string> Validate(RegionalEngagementBudget item)
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

    public void RequireValid(RegionalEngagementBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalEngagementBudget> items)
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

/// <summary>Functions of regional engagement budget rules.</summary>
public static class RegionalEngagementBudgetRules
{
    public static RegionalEngagementBudgetValidator DefaultRegionalEngagementBudgetValidator()
    {
        return new RegionalEngagementBudgetValidator(
            new List<IRegionalEngagementBudgetRule>
            {
                new RegionalEngagementBudgetWeightRangeRule(7.4, 3622.5),
                new RegionalEngagementBudgetRequiredSalaryBandRule("regional engagement budget needs a value"),
                new RegionalEngagementBudgetExpiresAtRangeRule(64, 255),
                new RegionalEngagementBudgetCodePrefixRule("OPS"),
                new RegionalEngagementBudgetRequiredLabelRule("regional engagement budget needs an entry"),
            });
    }
}
