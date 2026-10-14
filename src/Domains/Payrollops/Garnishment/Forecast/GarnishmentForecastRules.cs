namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment forecast must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentForecastRule
{
    List<string> Check(GarnishmentForecast item);
}

public sealed record GarnishmentForecastContactEmailLengthRule(int MaxLength) : IGarnishmentForecastRule
{
    public List<string> Check(GarnishmentForecast item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentForecastAllowedContactEmailRule(List<string> Allowed) : IGarnishmentForecastRule
{
    public List<string> Check(GarnishmentForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentForecastCodeLengthRule(int MaxLength) : IGarnishmentForecastRule
{
    public List<string> Check(GarnishmentForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentForecastStartsAtRangeRule(int Min, int Max) : IGarnishmentForecastRule
{
    public List<string> Check(GarnishmentForecast item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a garnishment forecast.</summary>
public sealed record GarnishmentForecastValidator(List<IGarnishmentForecastRule> Rules)
{
    public List<string> Validate(GarnishmentForecast item)
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

    public void RequireValid(GarnishmentForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentForecast> items)
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

/// <summary>Functions of garnishment forecast rules.</summary>
public static class GarnishmentForecastRules
{
    public static GarnishmentForecastValidator DefaultGarnishmentForecastValidator()
    {
        return new GarnishmentForecastValidator(
            new List<IGarnishmentForecastRule>
            {
                new GarnishmentForecastContactEmailLengthRule(120),
                new GarnishmentForecastAllowedContactEmailRule(new List<string> { "default", "scheduled" }),
                new GarnishmentForecastCodeLengthRule(86400),
                new GarnishmentForecastStartsAtRangeRule(255, 1000),
            });
    }
}
