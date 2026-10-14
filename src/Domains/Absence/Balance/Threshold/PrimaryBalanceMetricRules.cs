namespace Acme.Workforce.Absence.Balance.Threshold;

/// <summary>A rule a primary balance metric must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBalanceMetricRule
{
    List<string> Check(PrimaryBalanceMetric item);
}

public sealed record PrimaryBalanceMetricRequiredLocaleRule(string Message) : IPrimaryBalanceMetricRule
{
    public List<string> Check(PrimaryBalanceMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBalanceMetricRequiredTitleRule(string Message) : IPrimaryBalanceMetricRule
{
    public List<string> Check(PrimaryBalanceMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBalanceMetricRequiredCodeRule(string Message) : IPrimaryBalanceMetricRule
{
    public List<string> Check(PrimaryBalanceMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBalanceMetricAmountRangeRule(double Min, double Max) : IPrimaryBalanceMetricRule
{
    public List<string> Check(PrimaryBalanceMetric item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBalanceMetricLocaleLengthRule(int MaxLength) : IPrimaryBalanceMetricRule
{
    public List<string> Check(PrimaryBalanceMetric item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary balance metric.</summary>
public sealed record PrimaryBalanceMetricValidator(List<IPrimaryBalanceMetricRule> Rules)
{
    public List<string> Validate(PrimaryBalanceMetric item)
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

    public void RequireValid(PrimaryBalanceMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBalanceMetric> items)
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

/// <summary>Functions of primary balance metric rules.</summary>
public static class PrimaryBalanceMetricRules
{
    public static PrimaryBalanceMetricValidator DefaultPrimaryBalanceMetricValidator()
    {
        return new PrimaryBalanceMetricValidator(
            new List<IPrimaryBalanceMetricRule>
            {
                new PrimaryBalanceMetricRequiredLocaleRule("primary balance metric needs an entry"),
                new PrimaryBalanceMetricRequiredTitleRule("primary balance metric needs a value"),
                new PrimaryBalanceMetricRequiredCodeRule("primary balance metric needs a value"),
                new PrimaryBalanceMetricAmountRangeRule(4.0, 1597.3),
                new PrimaryBalanceMetricLocaleLengthRule(255),
            });
    }
}
