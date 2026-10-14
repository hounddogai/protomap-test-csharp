namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due metric must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueMetricRule
{
    List<string> Check(TrainingDueMetric item);
}

public sealed record TrainingDueMetricCodeLengthRule(int MaxLength) : ITrainingDueMetricRule
{
    public List<string> Check(TrainingDueMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMetricPhoneLengthRule(int MaxLength) : ITrainingDueMetricRule
{
    public List<string> Check(TrainingDueMetric item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMetricTitleLengthRule(int MaxLength) : ITrainingDueMetricRule
{
    public List<string> Check(TrainingDueMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMetricAllowedSalaryBandRule(List<string> Allowed) : ITrainingDueMetricRule
{
    public List<string> Check(TrainingDueMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is missing" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMetricCodePrefixRule(string Prefix) : ITrainingDueMetricRule
{
    public List<string> Check(TrainingDueMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due metric.</summary>
public sealed record TrainingDueMetricValidator(List<ITrainingDueMetricRule> Rules)
{
    public List<string> Validate(TrainingDueMetric item)
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

    public void RequireValid(TrainingDueMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueMetric> items)
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

/// <summary>Functions of training due metric rules.</summary>
public static class TrainingDueMetricRules
{
    public static TrainingDueMetricValidator DefaultTrainingDueMetricValidator()
    {
        return new TrainingDueMetricValidator(
            new List<ITrainingDueMetricRule>
            {
                new TrainingDueMetricCodeLengthRule(86400),
                new TrainingDueMetricPhoneLengthRule(120),
                new TrainingDueMetricTitleLengthRule(3600),
                new TrainingDueMetricAllowedSalaryBandRule(new List<string>()),
                new TrainingDueMetricCodePrefixRule("Q"),
            });
    }
}
