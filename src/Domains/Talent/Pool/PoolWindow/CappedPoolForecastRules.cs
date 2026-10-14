namespace Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>A rule a capped pool forecast must pass. Check returns its problems, or none.</summary>
public interface ICappedPoolForecastRule
{
    List<string> Check(CappedPoolForecast item);
}

public sealed record CappedPoolForecastFloorValueRangeRule(double Min, double Max) : ICappedPoolForecastRule
{
    public List<string> Check(CappedPoolForecast item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedPoolForecastCodeLengthRule(int MaxLength) : ICappedPoolForecastRule
{
    public List<string> Check(CappedPoolForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolForecastNameLengthRule(int MaxLength) : ICappedPoolForecastRule
{
    public List<string> Check(CappedPoolForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolForecastSequenceRangeRule(int Min, int Max) : ICappedPoolForecastRule
{
    public List<string> Check(CappedPoolForecast item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedPoolForecastDiscountRangeRule(double Min, double Max) : ICappedPoolForecastRule
{
    public List<string> Check(CappedPoolForecast item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pool forecast.</summary>
public sealed record CappedPoolForecastValidator(List<ICappedPoolForecastRule> Rules)
{
    public List<string> Validate(CappedPoolForecast item)
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

    public void RequireValid(CappedPoolForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPoolForecast> items)
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

/// <summary>Functions of capped pool forecast rules.</summary>
public static class CappedPoolForecastRules
{
    public static CappedPoolForecastValidator DefaultCappedPoolForecastValidator()
    {
        return new CappedPoolForecastValidator(
            new List<ICappedPoolForecastRule>
            {
                new CappedPoolForecastFloorValueRangeRule(8.0, 911.2),
                new CappedPoolForecastCodeLengthRule(32),
                new CappedPoolForecastNameLengthRule(32),
                new CappedPoolForecastSequenceRangeRule(64, 255),
                new CappedPoolForecastDiscountRangeRule(0.7, 3701.6),
            });
    }
}
