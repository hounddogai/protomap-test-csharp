namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>A rule a regional cost model summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalCostModelSummaryRule
{
    List<string> Check(RegionalCostModelSummary item);
}

public sealed record RegionalCostModelSummaryRequiredCurrencyRule(string Message) : IRegionalCostModelSummaryRule
{
    public List<string> Check(RegionalCostModelSummary item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCostModelSummaryRequiredCodeRule(string Message) : IRegionalCostModelSummaryRule
{
    public List<string> Check(RegionalCostModelSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCostModelSummaryCodePrefixRule(string Prefix) : IRegionalCostModelSummaryRule
{
    public List<string> Check(RegionalCostModelSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCostModelSummaryCurrencyLengthRule(int MaxLength) : IRegionalCostModelSummaryRule
{
    public List<string> Check(RegionalCostModelSummary item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCostModelSummaryRequiredTitleRule(string Message) : IRegionalCostModelSummaryRule
{
    public List<string> Check(RegionalCostModelSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional cost model summary.</summary>
public sealed record RegionalCostModelSummaryValidator(List<IRegionalCostModelSummaryRule> Rules)
{
    public List<string> Validate(RegionalCostModelSummary item)
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

    public void RequireValid(RegionalCostModelSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalCostModelSummary> items)
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

/// <summary>Functions of regional cost model summary rules.</summary>
public static class RegionalCostModelSummaryRules
{
    public static RegionalCostModelSummaryValidator DefaultRegionalCostModelSummaryValidator()
    {
        return new RegionalCostModelSummaryValidator(
            new List<IRegionalCostModelSummaryRule>
            {
                new RegionalCostModelSummaryRequiredCurrencyRule("regional cost model summary needs a value"),
                new RegionalCostModelSummaryRequiredCodeRule("regional cost model summary needs this field"),
                new RegionalCostModelSummaryCodePrefixRule("OPS"),
                new RegionalCostModelSummaryCurrencyLengthRule(86400),
                new RegionalCostModelSummaryRequiredTitleRule("regional cost model summary needs this field"),
            });
    }
}
