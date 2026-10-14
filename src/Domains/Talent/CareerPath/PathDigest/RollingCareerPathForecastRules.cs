namespace Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>A rule a rolling career path forecast must pass. Check returns its problems, or none.</summary>
public interface IRollingCareerPathForecastRule
{
    List<string> Check(RollingCareerPathForecast item);
}

public sealed record RollingCareerPathForecastTenureMonthsRangeRule(int Min, int Max) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingCareerPathForecastRequiredCodeRule(string Message) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathForecastRequiredTitleRule(string Message) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathForecastCodePrefixRule(string Prefix) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathForecastNameLengthRule(int MaxLength) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCareerPathForecastLeaveDaysRangeRule(int Min, int Max) : IRollingCareerPathForecastRule
{
    public List<string> Check(RollingCareerPathForecast item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling career path forecast.</summary>
public sealed record RollingCareerPathForecastValidator(List<IRollingCareerPathForecastRule> Rules)
{
    public List<string> Validate(RollingCareerPathForecast item)
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

    public void RequireValid(RollingCareerPathForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCareerPathForecast> items)
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

/// <summary>Functions of rolling career path forecast rules.</summary>
public static class RollingCareerPathForecastRules
{
    public static RollingCareerPathForecastValidator DefaultRollingCareerPathForecastValidator()
    {
        return new RollingCareerPathForecastValidator(
            new List<IRollingCareerPathForecastRule>
            {
                new RollingCareerPathForecastTenureMonthsRangeRule(16, 32),
                new RollingCareerPathForecastRequiredCodeRule("rolling career path forecast needs a value"),
                new RollingCareerPathForecastRequiredTitleRule("rolling career path forecast needs a value"),
                new RollingCareerPathForecastCodePrefixRule("REF"),
                new RollingCareerPathForecastNameLengthRule(3600),
                new RollingCareerPathForecastLeaveDaysRangeRule(16, 86400),
            });
    }
}
