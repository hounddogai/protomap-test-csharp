namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path forecast must pass. Check returns its problems, or none.</summary>
public interface ICareerPathForecastRule
{
    List<string> Check(CareerPathForecast item);
}

public sealed record CareerPathForecastRequiredBadgeRule(string Message) : ICareerPathForecastRule
{
    public List<string> Check(CareerPathForecast item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathForecastRequiredCodeRule(string Message) : ICareerPathForecastRule
{
    public List<string> Check(CareerPathForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathForecastMaximumRangeRule(double Min, double Max) : ICareerPathForecastRule
{
    public List<string> Check(CareerPathForecast item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathForecastTitleLengthRule(int MaxLength) : ICareerPathForecastRule
{
    public List<string> Check(CareerPathForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathForecastAllowedChannelRule(List<string> Allowed) : ICareerPathForecastRule
{
    public List<string> Check(CareerPathForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path forecast.</summary>
public sealed record CareerPathForecastValidator(List<ICareerPathForecastRule> Rules)
{
    public List<string> Validate(CareerPathForecast item)
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

    public void RequireValid(CareerPathForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathForecast> items)
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

/// <summary>Functions of career path forecast rules.</summary>
public static class CareerPathForecastRules
{
    public static CareerPathForecastValidator DefaultCareerPathForecastValidator()
    {
        return new CareerPathForecastValidator(
            new List<ICareerPathForecastRule>
            {
                new CareerPathForecastRequiredBadgeRule("career path forecast needs an entry"),
                new CareerPathForecastRequiredCodeRule("career path forecast needs a value"),
                new CareerPathForecastMaximumRangeRule(4.2, 4800.5),
                new CareerPathForecastTitleLengthRule(3600),
                new CareerPathForecastAllowedChannelRule(new List<string>()),
            });
    }
}
