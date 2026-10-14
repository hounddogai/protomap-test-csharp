namespace Acme.Workforce.Absence.Balance.Batch;

/// <summary>A rule a balance metric must pass. Check returns its problems, or none.</summary>
public interface IBalanceMetricRule
{
    List<string> Check(BalanceMetric item);
}

public sealed record BalanceMetricMinimumRangeRule(double Min, double Max) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceMetricTitleLengthRule(int MaxLength) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceMetricManagerIdLengthRule(int MaxLength) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceMetricRequiredManagerIdRule(string Message) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceMetricCountryLengthRule(int MaxLength) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceMetricCodeLengthRule(int MaxLength) : IBalanceMetricRule
{
    public List<string> Check(BalanceMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance metric.</summary>
public sealed record BalanceMetricValidator(List<IBalanceMetricRule> Rules)
{
    public List<string> Validate(BalanceMetric item)
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

    public void RequireValid(BalanceMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceMetric> items)
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

/// <summary>Functions of balance metric rules.</summary>
public static class BalanceMetricRules
{
    public static BalanceMetricValidator DefaultBalanceMetricValidator()
    {
        return new BalanceMetricValidator(
            new List<IBalanceMetricRule>
            {
                new BalanceMetricMinimumRangeRule(7.0, 3572.7),
                new BalanceMetricTitleLengthRule(86400),
                new BalanceMetricManagerIdLengthRule(120),
                new BalanceMetricRequiredManagerIdRule("balance metric needs an entry"),
                new BalanceMetricCountryLengthRule(1000),
                new BalanceMetricCodeLengthRule(16),
            });
    }
}
