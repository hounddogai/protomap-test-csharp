namespace Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>A rule a capped desk forecast must pass. Check returns its problems, or none.</summary>
public interface ICappedDeskForecastRule
{
    List<string> Check(CappedDeskForecast item);
}

public sealed record CappedDeskForecastJobCodeLengthRule(int MaxLength) : ICappedDeskForecastRule
{
    public List<string> Check(CappedDeskForecast item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskForecastNameLengthRule(int MaxLength) : ICappedDeskForecastRule
{
    public List<string> Check(CappedDeskForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskForecastCodeLengthRule(int MaxLength) : ICappedDeskForecastRule
{
    public List<string> Check(CappedDeskForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped desk forecast.</summary>
public sealed record CappedDeskForecastValidator(List<ICappedDeskForecastRule> Rules)
{
    public List<string> Validate(CappedDeskForecast item)
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

    public void RequireValid(CappedDeskForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeskForecast> items)
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

/// <summary>Functions of capped desk forecast rules.</summary>
public static class CappedDeskForecastRules
{
    public static CappedDeskForecastValidator DefaultCappedDeskForecastValidator()
    {
        return new CappedDeskForecastValidator(
            new List<ICappedDeskForecastRule>
            {
                new CappedDeskForecastJobCodeLengthRule(255),
                new CappedDeskForecastNameLengthRule(255),
                new CappedDeskForecastCodeLengthRule(1000),
            });
    }
}
