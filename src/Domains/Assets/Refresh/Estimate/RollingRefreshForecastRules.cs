namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling refresh forecast must pass. Check returns its problems, or none.</summary>
public interface IRollingRefreshForecastRule
{
    List<string> Check(RollingRefreshForecast item);
}

public sealed record RollingRefreshForecastDepartmentLengthRule(int MaxLength) : IRollingRefreshForecastRule
{
    public List<string> Check(RollingRefreshForecast item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshForecastTitleLengthRule(int MaxLength) : IRollingRefreshForecastRule
{
    public List<string> Check(RollingRefreshForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshForecastCodeLengthRule(int MaxLength) : IRollingRefreshForecastRule
{
    public List<string> Check(RollingRefreshForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshForecastLimitRangeRule(int Min, int Max) : IRollingRefreshForecastRule
{
    public List<string> Check(RollingRefreshForecast item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshForecastAllowedTitleRule(List<string> Allowed) : IRollingRefreshForecastRule
{
    public List<string> Check(RollingRefreshForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling refresh forecast.</summary>
public sealed record RollingRefreshForecastValidator(List<IRollingRefreshForecastRule> Rules)
{
    public List<string> Validate(RollingRefreshForecast item)
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

    public void RequireValid(RollingRefreshForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRefreshForecast> items)
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

/// <summary>Functions of rolling refresh forecast rules.</summary>
public static class RollingRefreshForecastRules
{
    public static RollingRefreshForecastValidator DefaultRollingRefreshForecastValidator()
    {
        return new RollingRefreshForecastValidator(
            new List<IRollingRefreshForecastRule>
            {
                new RollingRefreshForecastDepartmentLengthRule(120),
                new RollingRefreshForecastTitleLengthRule(3600),
                new RollingRefreshForecastCodeLengthRule(86400),
                new RollingRefreshForecastLimitRangeRule(120, 1000),
                new RollingRefreshForecastAllowedTitleRule(new List<string> { "scheduled" }),
            });
    }
}
