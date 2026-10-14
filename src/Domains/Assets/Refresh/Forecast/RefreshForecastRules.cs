namespace Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>A rule a refresh forecast must pass. Check returns its problems, or none.</summary>
public interface IRefreshForecastRule
{
    List<string> Check(RefreshForecast item);
}

public sealed record RefreshForecastRequiredChannelRule(string Message) : IRefreshForecastRule
{
    public List<string> Check(RefreshForecast item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshForecastRatioRangeRule(double Min, double Max) : IRefreshForecastRule
{
    public List<string> Check(RefreshForecast item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshForecastTitleLengthRule(int MaxLength) : IRefreshForecastRule
{
    public List<string> Check(RefreshForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshForecastRequiredCodeRule(string Message) : IRefreshForecastRule
{
    public List<string> Check(RefreshForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh forecast.</summary>
public sealed record RefreshForecastValidator(List<IRefreshForecastRule> Rules)
{
    public List<string> Validate(RefreshForecast item)
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

    public void RequireValid(RefreshForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshForecast> items)
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

/// <summary>Functions of refresh forecast rules.</summary>
public static class RefreshForecastRules
{
    public static RefreshForecastValidator DefaultRefreshForecastValidator()
    {
        return new RefreshForecastValidator(
            new List<IRefreshForecastRule>
            {
                new RefreshForecastRequiredChannelRule("refresh forecast needs this field"),
                new RefreshForecastRatioRangeRule(1.7, 2107.0),
                new RefreshForecastTitleLengthRule(16),
                new RefreshForecastRequiredCodeRule("refresh forecast needs an entry"),
            });
    }
}
