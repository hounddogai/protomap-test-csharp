namespace Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>A rule a pay band metric must pass. Check returns its problems, or none.</summary>
public interface IPayBandMetricRule
{
    List<string> Check(PayBandMetric item);
}

public sealed record PayBandMetricAmountRangeRule(double Min, double Max) : IPayBandMetricRule
{
    public List<string> Check(PayBandMetric item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMetricRequiredExternalRefRule(string Message) : IPayBandMetricRule
{
    public List<string> Check(PayBandMetric item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandMetricRatioRangeRule(double Min, double Max) : IPayBandMetricRule
{
    public List<string> Check(PayBandMetric item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMetricCodePrefixRule(string Prefix) : IPayBandMetricRule
{
    public List<string> Check(PayBandMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMetricCodeLengthRule(int MaxLength) : IPayBandMetricRule
{
    public List<string> Check(PayBandMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band metric.</summary>
public sealed record PayBandMetricValidator(List<IPayBandMetricRule> Rules)
{
    public List<string> Validate(PayBandMetric item)
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

    public void RequireValid(PayBandMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandMetric> items)
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

/// <summary>Functions of pay band metric rules.</summary>
public static class PayBandMetricRules
{
    public static PayBandMetricValidator DefaultPayBandMetricValidator()
    {
        return new PayBandMetricValidator(
            new List<IPayBandMetricRule>
            {
                new PayBandMetricAmountRangeRule(7.8, 507.3),
                new PayBandMetricRequiredExternalRefRule("pay band metric needs a value"),
                new PayBandMetricRatioRangeRule(9.6, 1560.5),
                new PayBandMetricCodePrefixRule("X"),
                new PayBandMetricCodeLengthRule(120),
            });
    }
}
