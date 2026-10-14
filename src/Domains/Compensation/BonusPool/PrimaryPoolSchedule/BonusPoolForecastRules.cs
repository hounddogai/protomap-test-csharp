namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>A rule a bonus pool forecast must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolForecastRule
{
    List<string> Check(BonusPoolForecast item);
}

public sealed record BonusPoolForecastPinnedGuardRule(int Limit) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        if (item.Pinned && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolForecastRequiredTitleRule(string Message) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolForecastSummaryLengthRule(int MaxLength) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolForecastRequiredSummaryRule(string Message) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolForecastTitleLengthRule(int MaxLength) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolForecastRevisionRangeRule(int Min, int Max) : IBonusPoolForecastRule
{
    public List<string> Check(BonusPoolForecast item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool forecast.</summary>
public sealed record BonusPoolForecastValidator(List<IBonusPoolForecastRule> Rules)
{
    public List<string> Validate(BonusPoolForecast item)
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

    public void RequireValid(BonusPoolForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolForecast> items)
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

/// <summary>Functions of bonus pool forecast rules.</summary>
public static class BonusPoolForecastRules
{
    public static BonusPoolForecastValidator DefaultBonusPoolForecastValidator()
    {
        return new BonusPoolForecastValidator(
            new List<IBonusPoolForecastRule>
            {
                new BonusPoolForecastPinnedGuardRule(64),
                new BonusPoolForecastRequiredTitleRule("bonus pool forecast needs an entry"),
                new BonusPoolForecastSummaryLengthRule(1000),
                new BonusPoolForecastRequiredSummaryRule("bonus pool forecast needs a value"),
                new BonusPoolForecastTitleLengthRule(1000),
                new BonusPoolForecastRevisionRangeRule(1000, 1000),
            });
    }
}
