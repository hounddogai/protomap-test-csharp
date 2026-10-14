namespace Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>A rule a primary pay band forecast must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPayBandForecastRule
{
    List<string> Check(PrimaryPayBandForecast item);
}

public sealed record PrimaryPayBandForecastRequiredCodeRule(string Message) : IPrimaryPayBandForecastRule
{
    public List<string> Check(PrimaryPayBandForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandForecastScoreRangeRule(double Min, double Max) : IPrimaryPayBandForecastRule
{
    public List<string> Check(PrimaryPayBandForecast item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandForecastLabelLengthRule(int MaxLength) : IPrimaryPayBandForecastRule
{
    public List<string> Check(PrimaryPayBandForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandForecastCodeLengthRule(int MaxLength) : IPrimaryPayBandForecastRule
{
    public List<string> Check(PrimaryPayBandForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayBandForecastCodePrefixRule(string Prefix) : IPrimaryPayBandForecastRule
{
    public List<string> Check(PrimaryPayBandForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary pay band forecast.</summary>
public sealed record PrimaryPayBandForecastValidator(List<IPrimaryPayBandForecastRule> Rules)
{
    public List<string> Validate(PrimaryPayBandForecast item)
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

    public void RequireValid(PrimaryPayBandForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPayBandForecast> items)
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

/// <summary>Functions of primary pay band forecast rules.</summary>
public static class PrimaryPayBandForecastRules
{
    public static PrimaryPayBandForecastValidator DefaultPrimaryPayBandForecastValidator()
    {
        return new PrimaryPayBandForecastValidator(
            new List<IPrimaryPayBandForecastRule>
            {
                new PrimaryPayBandForecastRequiredCodeRule("primary pay band forecast needs an entry"),
                new PrimaryPayBandForecastScoreRangeRule(5.8, 1266.3),
                new PrimaryPayBandForecastLabelLengthRule(3600),
                new PrimaryPayBandForecastCodeLengthRule(86400),
                new PrimaryPayBandForecastCodePrefixRule("ACM"),
            });
    }
}
