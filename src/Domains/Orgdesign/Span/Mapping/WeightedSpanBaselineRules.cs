namespace Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>A rule a weighted span baseline must pass. Check returns its problems, or none.</summary>
public interface IWeightedSpanBaselineRule
{
    List<string> Check(WeightedSpanBaseline item);
}

public sealed record WeightedSpanBaselineTitleLengthRule(int MaxLength) : IWeightedSpanBaselineRule
{
    public List<string> Check(WeightedSpanBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSpanBaselineRequiredTitleRule(string Message) : IWeightedSpanBaselineRule
{
    public List<string> Check(WeightedSpanBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedSpanBaselineSalaryBandLengthRule(int MaxLength) : IWeightedSpanBaselineRule
{
    public List<string> Check(WeightedSpanBaseline item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted span baseline.</summary>
public sealed record WeightedSpanBaselineValidator(List<IWeightedSpanBaselineRule> Rules)
{
    public List<string> Validate(WeightedSpanBaseline item)
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

    public void RequireValid(WeightedSpanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedSpanBaseline> items)
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

/// <summary>Functions of weighted span baseline rules.</summary>
public static class WeightedSpanBaselineRules
{
    public static WeightedSpanBaselineValidator DefaultWeightedSpanBaselineValidator()
    {
        return new WeightedSpanBaselineValidator(
            new List<IWeightedSpanBaselineRule>
            {
                new WeightedSpanBaselineTitleLengthRule(16),
                new WeightedSpanBaselineRequiredTitleRule("weighted span baseline needs an entry"),
                new WeightedSpanBaselineSalaryBandLengthRule(120),
            });
    }
}
