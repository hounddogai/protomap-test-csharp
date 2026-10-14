namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global training due forecast must pass. Check returns its problems, or none.</summary>
public interface IGlobalTrainingDueForecastRule
{
    List<string> Check(GlobalTrainingDueForecast item);
}

public sealed record GlobalTrainingDueForecastTimezoneLengthRule(int MaxLength) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTrainingDueForecastRateRangeRule(double Min, double Max) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalTrainingDueForecastAllowedTitleRule(List<string> Allowed) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTrainingDueForecastTitleLengthRule(int MaxLength) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTrainingDueForecastCeilingRangeRule(double Min, double Max) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalTrainingDueForecastRequiredTitleRule(string Message) : IGlobalTrainingDueForecastRule
{
    public List<string> Check(GlobalTrainingDueForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global training due forecast.</summary>
public sealed record GlobalTrainingDueForecastValidator(List<IGlobalTrainingDueForecastRule> Rules)
{
    public List<string> Validate(GlobalTrainingDueForecast item)
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

    public void RequireValid(GlobalTrainingDueForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalTrainingDueForecast> items)
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

/// <summary>Functions of global training due forecast rules.</summary>
public static class GlobalTrainingDueForecastRules
{
    public static GlobalTrainingDueForecastValidator DefaultGlobalTrainingDueForecastValidator()
    {
        return new GlobalTrainingDueForecastValidator(
            new List<IGlobalTrainingDueForecastRule>
            {
                new GlobalTrainingDueForecastTimezoneLengthRule(64),
                new GlobalTrainingDueForecastRateRangeRule(9.4, 3972.3),
                new GlobalTrainingDueForecastAllowedTitleRule(new List<string> { "legacy" }),
                new GlobalTrainingDueForecastTitleLengthRule(64),
                new GlobalTrainingDueForecastCeilingRangeRule(6.4, 4110.4),
                new GlobalTrainingDueForecastRequiredTitleRule("global training due forecast needs an entry"),
            });
    }
}
