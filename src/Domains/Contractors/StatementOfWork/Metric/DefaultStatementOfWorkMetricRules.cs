namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

/// <summary>A rule a default statement of work metric must pass. Check returns its problems, or none.</summary>
public interface IDefaultStatementOfWorkMetricRule
{
    List<string> Check(DefaultStatementOfWorkMetric item);
}

public sealed record DefaultStatementOfWorkMetricRequiredNameRule(string Message) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMetricRequiredCountryRule(string Message) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMetricCodeLengthRule(int MaxLength) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMetricPinnedGuardRule(int Limit) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMetricQuantityRangeRule(int Min, int Max) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkMetricCountryLengthRule(int MaxLength) : IDefaultStatementOfWorkMetricRule
{
    public List<string> Check(DefaultStatementOfWorkMetric item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default statement of work metric.</summary>
public sealed record DefaultStatementOfWorkMetricValidator(List<IDefaultStatementOfWorkMetricRule> Rules)
{
    public List<string> Validate(DefaultStatementOfWorkMetric item)
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

    public void RequireValid(DefaultStatementOfWorkMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultStatementOfWorkMetric> items)
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

/// <summary>Functions of default statement of work metric rules.</summary>
public static class DefaultStatementOfWorkMetricRules
{
    public static DefaultStatementOfWorkMetricValidator DefaultDefaultStatementOfWorkMetricValidator()
    {
        return new DefaultStatementOfWorkMetricValidator(
            new List<IDefaultStatementOfWorkMetricRule>
            {
                new DefaultStatementOfWorkMetricRequiredNameRule("default statement of work metric needs an entry"),
                new DefaultStatementOfWorkMetricRequiredCountryRule(
                    "default statement of work metric needs an entry"),
                new DefaultStatementOfWorkMetricCodeLengthRule(32),
                new DefaultStatementOfWorkMetricPinnedGuardRule(16),
                new DefaultStatementOfWorkMetricQuantityRangeRule(32, 3600),
                new DefaultStatementOfWorkMetricCountryLengthRule(16),
            });
    }
}
