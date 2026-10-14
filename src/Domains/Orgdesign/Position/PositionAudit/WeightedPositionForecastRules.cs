namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>A rule a weighted position forecast must pass. Check returns its problems, or none.</summary>
public interface IWeightedPositionForecastRule
{
    List<string> Check(WeightedPositionForecast item);
}

public sealed record WeightedPositionForecastTimezoneLengthRule(int MaxLength) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPositionForecastLabelLengthRule(int MaxLength) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPositionForecastApprovedGuardRule(int Limit) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        if (item.Approved && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPositionForecastCodePrefixRule(string Prefix) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPositionForecastVersionRangeRule(int Min, int Max) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPositionForecastRequiredLocationRule(string Message) : IWeightedPositionForecastRule
{
    public List<string> Check(WeightedPositionForecast item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted position forecast.</summary>
public sealed record WeightedPositionForecastValidator(List<IWeightedPositionForecastRule> Rules)
{
    public List<string> Validate(WeightedPositionForecast item)
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

    public void RequireValid(WeightedPositionForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPositionForecast> items)
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

/// <summary>Functions of weighted position forecast rules.</summary>
public static class WeightedPositionForecastRules
{
    public static WeightedPositionForecastValidator DefaultWeightedPositionForecastValidator()
    {
        return new WeightedPositionForecastValidator(
            new List<IWeightedPositionForecastRule>
            {
                new WeightedPositionForecastTimezoneLengthRule(3600),
                new WeightedPositionForecastLabelLengthRule(64),
                new WeightedPositionForecastApprovedGuardRule(255),
                new WeightedPositionForecastCodePrefixRule("OPS"),
                new WeightedPositionForecastVersionRangeRule(255, 86400),
                new WeightedPositionForecastRequiredLocationRule("weighted position forecast needs an entry"),
            });
    }
}
