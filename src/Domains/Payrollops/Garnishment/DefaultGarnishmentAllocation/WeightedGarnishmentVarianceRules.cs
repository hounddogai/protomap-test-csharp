namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>A rule a weighted garnishment variance must pass. Check returns its problems, or none.</summary>
public interface IWeightedGarnishmentVarianceRule
{
    List<string> Check(WeightedGarnishmentVariance item);
}

public sealed record WeightedGarnishmentVarianceNameLengthRule(int MaxLength) : IWeightedGarnishmentVarianceRule
{
    public List<string> Check(WeightedGarnishmentVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentVarianceCreatedAtRangeRule(int Min, int Max) : IWeightedGarnishmentVarianceRule
{
    public List<string> Check(WeightedGarnishmentVariance item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentVarianceRequiredSegmentRule(string Message) : IWeightedGarnishmentVarianceRule
{
    public List<string> Check(WeightedGarnishmentVariance item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted garnishment variance.</summary>
public sealed record WeightedGarnishmentVarianceValidator(List<IWeightedGarnishmentVarianceRule> Rules)
{
    public List<string> Validate(WeightedGarnishmentVariance item)
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

    public void RequireValid(WeightedGarnishmentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedGarnishmentVariance> items)
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

/// <summary>Functions of weighted garnishment variance rules.</summary>
public static class WeightedGarnishmentVarianceRules
{
    public static WeightedGarnishmentVarianceValidator DefaultWeightedGarnishmentVarianceValidator()
    {
        return new WeightedGarnishmentVarianceValidator(
            new List<IWeightedGarnishmentVarianceRule>
            {
                new WeightedGarnishmentVarianceNameLengthRule(86400),
                new WeightedGarnishmentVarianceCreatedAtRangeRule(16, 16),
                new WeightedGarnishmentVarianceRequiredSegmentRule("weighted garnishment variance needs a value"),
            });
    }
}
