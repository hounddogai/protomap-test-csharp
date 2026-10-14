namespace Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>A rule a adaptive retro pay metric must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveRetroPayMetricRule
{
    List<string> Check(AdaptiveRetroPayMetric item);
}

public sealed record AdaptiveRetroPayMetricAmountRangeRule(double Min, double Max) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveRetroPayMetricCodeLengthRule(int MaxLength) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayMetricRequiredCodeRule(string Message) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayMetricBudgetRangeRule(double Min, double Max) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveRetroPayMetricCodePrefixRule(string Prefix) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayMetricRequiredTitleRule(string Message) : IAdaptiveRetroPayMetricRule
{
    public List<string> Check(AdaptiveRetroPayMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive retro pay metric.</summary>
public sealed record AdaptiveRetroPayMetricValidator(List<IAdaptiveRetroPayMetricRule> Rules)
{
    public List<string> Validate(AdaptiveRetroPayMetric item)
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

    public void RequireValid(AdaptiveRetroPayMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveRetroPayMetric> items)
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

/// <summary>Functions of adaptive retro pay metric rules.</summary>
public static class AdaptiveRetroPayMetricRules
{
    public static AdaptiveRetroPayMetricValidator DefaultAdaptiveRetroPayMetricValidator()
    {
        return new AdaptiveRetroPayMetricValidator(
            new List<IAdaptiveRetroPayMetricRule>
            {
                new AdaptiveRetroPayMetricAmountRangeRule(4.4, 2111.1),
                new AdaptiveRetroPayMetricCodeLengthRule(120),
                new AdaptiveRetroPayMetricRequiredCodeRule("adaptive retro pay metric needs this field"),
                new AdaptiveRetroPayMetricBudgetRangeRule(8.6, 3306.5),
                new AdaptiveRetroPayMetricCodePrefixRule("REF"),
                new AdaptiveRetroPayMetricRequiredTitleRule("adaptive retro pay metric needs a value"),
            });
    }
}
