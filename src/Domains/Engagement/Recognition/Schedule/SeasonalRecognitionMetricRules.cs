namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal recognition metric must pass. Check returns its problems, or none.</summary>
public interface ISeasonalRecognitionMetricRule
{
    List<string> Check(SeasonalRecognitionMetric item);
}

public sealed record SeasonalRecognitionMetricAllowedTitleRule(List<string> Allowed) : ISeasonalRecognitionMetricRule
{
    public List<string> Check(SeasonalRecognitionMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRecognitionMetricRequiredDescriptionRule(string Message) : ISeasonalRecognitionMetricRule
{
    public List<string> Check(SeasonalRecognitionMetric item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRecognitionMetricCodeLengthRule(int MaxLength) : ISeasonalRecognitionMetricRule
{
    public List<string> Check(SeasonalRecognitionMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal recognition metric.</summary>
public sealed record SeasonalRecognitionMetricValidator(List<ISeasonalRecognitionMetricRule> Rules)
{
    public List<string> Validate(SeasonalRecognitionMetric item)
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

    public void RequireValid(SeasonalRecognitionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalRecognitionMetric> items)
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

/// <summary>Functions of seasonal recognition metric rules.</summary>
public static class SeasonalRecognitionMetricRules
{
    public static SeasonalRecognitionMetricValidator DefaultSeasonalRecognitionMetricValidator()
    {
        return new SeasonalRecognitionMetricValidator(
            new List<ISeasonalRecognitionMetricRule>
            {
                new SeasonalRecognitionMetricAllowedTitleRule(new List<string>()),
                new SeasonalRecognitionMetricRequiredDescriptionRule("seasonal recognition metric needs a value"),
                new SeasonalRecognitionMetricCodeLengthRule(1000),
            });
    }
}
