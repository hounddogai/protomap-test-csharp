namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>A rule a visitor forecast must pass. Check returns its problems, or none.</summary>
public interface IVisitorForecastRule
{
    List<string> Check(VisitorForecast item);
}

public sealed record VisitorForecastCodePrefixRule(string Prefix) : IVisitorForecastRule
{
    public List<string> Check(VisitorForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorForecastBadgeLengthRule(int MaxLength) : IVisitorForecastRule
{
    public List<string> Check(VisitorForecast item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorForecastCodeLengthRule(int MaxLength) : IVisitorForecastRule
{
    public List<string> Check(VisitorForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorForecastRevisionRangeRule(int Min, int Max) : IVisitorForecastRule
{
    public List<string> Check(VisitorForecast item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorForecastUpdatedAtRangeRule(int Min, int Max) : IVisitorForecastRule
{
    public List<string> Check(VisitorForecast item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor forecast.</summary>
public sealed record VisitorForecastValidator(List<IVisitorForecastRule> Rules)
{
    public List<string> Validate(VisitorForecast item)
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

    public void RequireValid(VisitorForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorForecast> items)
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

/// <summary>Functions of visitor forecast rules.</summary>
public static class VisitorForecastRules
{
    public static VisitorForecastValidator DefaultVisitorForecastValidator()
    {
        return new VisitorForecastValidator(
            new List<IVisitorForecastRule>
            {
                new VisitorForecastCodePrefixRule("Q"),
                new VisitorForecastBadgeLengthRule(64),
                new VisitorForecastCodeLengthRule(64),
                new VisitorForecastRevisionRangeRule(80, 1000),
                new VisitorForecastUpdatedAtRangeRule(32, 255),
            });
    }
}
