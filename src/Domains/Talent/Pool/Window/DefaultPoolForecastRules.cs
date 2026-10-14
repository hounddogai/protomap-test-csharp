namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default pool forecast must pass. Check returns its problems, or none.</summary>
public interface IDefaultPoolForecastRule
{
    List<string> Check(DefaultPoolForecast item);
}

public sealed record DefaultPoolForecastLocationLengthRule(int MaxLength) : IDefaultPoolForecastRule
{
    public List<string> Check(DefaultPoolForecast item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolForecastAllowedLocationRule(List<string> Allowed) : IDefaultPoolForecastRule
{
    public List<string> Check(DefaultPoolForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolForecastRequiredCodeRule(string Message) : IDefaultPoolForecastRule
{
    public List<string> Check(DefaultPoolForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pool forecast.</summary>
public sealed record DefaultPoolForecastValidator(List<IDefaultPoolForecastRule> Rules)
{
    public List<string> Validate(DefaultPoolForecast item)
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

    public void RequireValid(DefaultPoolForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPoolForecast> items)
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

/// <summary>Functions of default pool forecast rules.</summary>
public static class DefaultPoolForecastRules
{
    public static DefaultPoolForecastValidator DefaultDefaultPoolForecastValidator()
    {
        return new DefaultPoolForecastValidator(
            new List<IDefaultPoolForecastRule>
            {
                new DefaultPoolForecastLocationLengthRule(3600),
                new DefaultPoolForecastAllowedLocationRule(new List<string>()),
                new DefaultPoolForecastRequiredCodeRule("default pool forecast needs an entry"),
            });
    }
}
