namespace Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>A rule a rolling asset return forecast must pass. Check returns its problems, or none.</summary>
public interface IRollingAssetReturnForecastRule
{
    List<string> Check(RollingAssetReturnForecast item);
}

public sealed record RollingAssetReturnForecastBadgeLengthRule(int MaxLength) : IRollingAssetReturnForecastRule
{
    public List<string> Check(RollingAssetReturnForecast item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnForecastRequiredOwnerIdRule(string Message) : IRollingAssetReturnForecastRule
{
    public List<string> Check(RollingAssetReturnForecast item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnForecastRequiredTitleRule(string Message) : IRollingAssetReturnForecastRule
{
    public List<string> Check(RollingAssetReturnForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling asset return forecast.</summary>
public sealed record RollingAssetReturnForecastValidator(List<IRollingAssetReturnForecastRule> Rules)
{
    public List<string> Validate(RollingAssetReturnForecast item)
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

    public void RequireValid(RollingAssetReturnForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAssetReturnForecast> items)
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

/// <summary>Functions of rolling asset return forecast rules.</summary>
public static class RollingAssetReturnForecastRules
{
    public static RollingAssetReturnForecastValidator DefaultRollingAssetReturnForecastValidator()
    {
        return new RollingAssetReturnForecastValidator(
            new List<IRollingAssetReturnForecastRule>
            {
                new RollingAssetReturnForecastBadgeLengthRule(86400),
                new RollingAssetReturnForecastRequiredOwnerIdRule("rolling asset return forecast needs an entry"),
                new RollingAssetReturnForecastRequiredTitleRule("rolling asset return forecast needs an entry"),
            });
    }
}
