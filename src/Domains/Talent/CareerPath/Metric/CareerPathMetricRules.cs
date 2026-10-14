namespace Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>A rule a career path metric must pass. Check returns its problems, or none.</summary>
public interface ICareerPathMetricRule
{
    List<string> Check(CareerPathMetric item);
}

public sealed record CareerPathMetricRequiredNotesRule(string Message) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMetricTimezoneLengthRule(int MaxLength) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMetricCodeLengthRule(int MaxLength) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMetricSequenceRangeRule(int Min, int Max) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
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

public sealed record CareerPathMetricNotesLengthRule(int MaxLength) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMetricCodePrefixRule(string Prefix) : ICareerPathMetricRule
{
    public List<string> Check(CareerPathMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path metric.</summary>
public sealed record CareerPathMetricValidator(List<ICareerPathMetricRule> Rules)
{
    public List<string> Validate(CareerPathMetric item)
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

    public void RequireValid(CareerPathMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathMetric> items)
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

/// <summary>Functions of career path metric rules.</summary>
public static class CareerPathMetricRules
{
    public static CareerPathMetricValidator DefaultCareerPathMetricValidator()
    {
        return new CareerPathMetricValidator(
            new List<ICareerPathMetricRule>
            {
                new CareerPathMetricRequiredNotesRule("career path metric needs this field"),
                new CareerPathMetricTimezoneLengthRule(255),
                new CareerPathMetricCodeLengthRule(1000),
                new CareerPathMetricSequenceRangeRule(120, 1000),
                new CareerPathMetricNotesLengthRule(80),
                new CareerPathMetricCodePrefixRule("Q"),
            });
    }
}
