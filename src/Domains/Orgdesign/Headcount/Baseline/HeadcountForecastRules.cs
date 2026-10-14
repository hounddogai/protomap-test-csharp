namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount forecast must pass. Check returns its problems, or none.</summary>
public interface IHeadcountForecastRule
{
    List<string> Check(HeadcountForecast item);
}

public sealed record HeadcountForecastStartsAtRangeRule(int Min, int Max) : IHeadcountForecastRule
{
    public List<string> Check(HeadcountForecast item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountForecastAllowedSalaryBandRule(List<string> Allowed) : IHeadcountForecastRule
{
    public List<string> Check(HeadcountForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is expired" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountForecastRequiredNameRule(string Message) : IHeadcountForecastRule
{
    public List<string> Check(HeadcountForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount forecast.</summary>
public sealed record HeadcountForecastValidator(List<IHeadcountForecastRule> Rules)
{
    public List<string> Validate(HeadcountForecast item)
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

    public void RequireValid(HeadcountForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountForecast> items)
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

/// <summary>Functions of headcount forecast rules.</summary>
public static class HeadcountForecastRules
{
    public static HeadcountForecastValidator DefaultHeadcountForecastValidator()
    {
        return new HeadcountForecastValidator(
            new List<IHeadcountForecastRule>
            {
                new HeadcountForecastStartsAtRangeRule(120, 120),
                new HeadcountForecastAllowedSalaryBandRule(new List<string>()),
                new HeadcountForecastRequiredNameRule("headcount forecast needs this field"),
            });
    }
}
