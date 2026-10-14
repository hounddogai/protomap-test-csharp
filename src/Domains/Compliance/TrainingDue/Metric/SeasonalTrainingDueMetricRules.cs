namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal training due metric must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTrainingDueMetricRule
{
    List<string> Check(SeasonalTrainingDueMetric item);
}

public sealed record SeasonalTrainingDueMetricActiveGuardRule(int Limit) : ISeasonalTrainingDueMetricRule
{
    public List<string> Check(SeasonalTrainingDueMetric item)
    {
        if (item.Active && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTrainingDueMetricRequiredManagerIdRule(string Message) : ISeasonalTrainingDueMetricRule
{
    public List<string> Check(SeasonalTrainingDueMetric item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTrainingDueMetricAllowedCodeRule(List<string> Allowed) : ISeasonalTrainingDueMetricRule
{
    public List<string> Check(SeasonalTrainingDueMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTrainingDueMetricManagerIdLengthRule(int MaxLength) : ISeasonalTrainingDueMetricRule
{
    public List<string> Check(SeasonalTrainingDueMetric item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal training due metric.</summary>
public sealed record SeasonalTrainingDueMetricValidator(List<ISeasonalTrainingDueMetricRule> Rules)
{
    public List<string> Validate(SeasonalTrainingDueMetric item)
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

    public void RequireValid(SeasonalTrainingDueMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTrainingDueMetric> items)
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

/// <summary>Functions of seasonal training due metric rules.</summary>
public static class SeasonalTrainingDueMetricRules
{
    public static SeasonalTrainingDueMetricValidator DefaultSeasonalTrainingDueMetricValidator()
    {
        return new SeasonalTrainingDueMetricValidator(
            new List<ISeasonalTrainingDueMetricRule>
            {
                new SeasonalTrainingDueMetricActiveGuardRule(16),
                new SeasonalTrainingDueMetricRequiredManagerIdRule("seasonal training due metric needs an entry"),
                new SeasonalTrainingDueMetricAllowedCodeRule(new List<string> { "strict", "rolling", "default" }),
                new SeasonalTrainingDueMetricManagerIdLengthRule(120),
            });
    }
}
