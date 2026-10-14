namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool forecast must pass. Check returns its problems, or none.</summary>
public interface IPoolForecastRule
{
    List<string> Check(PoolForecast item);
}

public sealed record PoolForecastDiscountRangeRule(double Min, double Max) : IPoolForecastRule
{
    public List<string> Check(PoolForecast item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolForecastAllowedCodeRule(List<string> Allowed) : IPoolForecastRule
{
    public List<string> Check(PoolForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PoolForecastCategoryLengthRule(int MaxLength) : IPoolForecastRule
{
    public List<string> Check(PoolForecast item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool forecast.</summary>
public sealed record PoolForecastValidator(List<IPoolForecastRule> Rules)
{
    public List<string> Validate(PoolForecast item)
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

    public void RequireValid(PoolForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolForecast> items)
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

/// <summary>Functions of pool forecast rules.</summary>
public static class PoolForecastRules
{
    public static PoolForecastValidator DefaultPoolForecastValidator()
    {
        return new PoolForecastValidator(
            new List<IPoolForecastRule>
            {
                new PoolForecastDiscountRangeRule(2.2, 511.6),
                new PoolForecastAllowedCodeRule(new List<string>()),
                new PoolForecastCategoryLengthRule(80),
            });
    }
}
