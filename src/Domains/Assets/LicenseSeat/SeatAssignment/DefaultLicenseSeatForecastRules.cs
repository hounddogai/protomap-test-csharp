namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>A rule a default license seat forecast must pass. Check returns its problems, or none.</summary>
public interface IDefaultLicenseSeatForecastRule
{
    List<string> Check(DefaultLicenseSeatForecast item);
}

public sealed record DefaultLicenseSeatForecastSkillLengthRule(int MaxLength) : IDefaultLicenseSeatForecastRule
{
    public List<string> Check(DefaultLicenseSeatForecast item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatForecastRequiredCodeRule(string Message) : IDefaultLicenseSeatForecastRule
{
    public List<string> Check(DefaultLicenseSeatForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatForecastBatchSizeRangeRule(int Min, int Max) : IDefaultLicenseSeatForecastRule
{
    public List<string> Check(DefaultLicenseSeatForecast item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default license seat forecast.</summary>
public sealed record DefaultLicenseSeatForecastValidator(List<IDefaultLicenseSeatForecastRule> Rules)
{
    public List<string> Validate(DefaultLicenseSeatForecast item)
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

    public void RequireValid(DefaultLicenseSeatForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLicenseSeatForecast> items)
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

/// <summary>Functions of default license seat forecast rules.</summary>
public static class DefaultLicenseSeatForecastRules
{
    public static DefaultLicenseSeatForecastValidator DefaultDefaultLicenseSeatForecastValidator()
    {
        return new DefaultLicenseSeatForecastValidator(
            new List<IDefaultLicenseSeatForecastRule>
            {
                new DefaultLicenseSeatForecastSkillLengthRule(86400),
                new DefaultLicenseSeatForecastRequiredCodeRule("default license seat forecast needs an entry"),
                new DefaultLicenseSeatForecastBatchSizeRangeRule(80, 86400),
            });
    }
}
