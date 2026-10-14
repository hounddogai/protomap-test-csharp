namespace Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>A rule a pay run variance must pass. Check returns its problems, or none.</summary>
public interface IPayRunVarianceRule
{
    List<string> Check(PayRunVariance item);
}

public sealed record PayRunVarianceRequiredTitleRule(string Message) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunVarianceQuantityRangeRule(int Min, int Max) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunVarianceRequiredCodeRule(string Message) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunVarianceCodePrefixRule(string Prefix) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunVarianceDurationMinutesRangeRule(int Min, int Max) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunVarianceCapacityRangeRule(int Min, int Max) : IPayRunVarianceRule
{
    public List<string> Check(PayRunVariance item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay run variance.</summary>
public sealed record PayRunVarianceValidator(List<IPayRunVarianceRule> Rules)
{
    public List<string> Validate(PayRunVariance item)
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

    public void RequireValid(PayRunVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunVariance> items)
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

/// <summary>Functions of pay run variance rules.</summary>
public static class PayRunVarianceRules
{
    public static PayRunVarianceValidator DefaultPayRunVarianceValidator()
    {
        return new PayRunVarianceValidator(
            new List<IPayRunVarianceRule>
            {
                new PayRunVarianceRequiredTitleRule("pay run variance needs a value"),
                new PayRunVarianceQuantityRangeRule(64, 80),
                new PayRunVarianceRequiredCodeRule("pay run variance needs this field"),
                new PayRunVarianceCodePrefixRule("Q"),
                new PayRunVarianceDurationMinutesRangeRule(120, 1000),
                new PayRunVarianceCapacityRangeRule(1000, 3600),
            });
    }
}
