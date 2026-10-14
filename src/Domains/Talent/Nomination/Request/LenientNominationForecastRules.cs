namespace Acme.Workforce.Talent.Nomination.Request;

/// <summary>A rule a lenient nomination forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientNominationForecastRule
{
    List<string> Check(LenientNominationForecast item);
}

public sealed record LenientNominationForecastTenureMonthsRangeRule(int Min, int Max) : ILenientNominationForecastRule
{
    public List<string> Check(LenientNominationForecast item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationForecastCodePrefixRule(string Prefix) : ILenientNominationForecastRule
{
    public List<string> Check(LenientNominationForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationForecastCreatedAtRangeRule(int Min, int Max) : ILenientNominationForecastRule
{
    public List<string> Check(LenientNominationForecast item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient nomination forecast.</summary>
public sealed record LenientNominationForecastValidator(List<ILenientNominationForecastRule> Rules)
{
    public List<string> Validate(LenientNominationForecast item)
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

    public void RequireValid(LenientNominationForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientNominationForecast> items)
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

/// <summary>Functions of lenient nomination forecast rules.</summary>
public static class LenientNominationForecastRules
{
    public static LenientNominationForecastValidator DefaultLenientNominationForecastValidator()
    {
        return new LenientNominationForecastValidator(
            new List<ILenientNominationForecastRule>
            {
                new LenientNominationForecastTenureMonthsRangeRule(1000, 86400),
                new LenientNominationForecastCodePrefixRule("X"),
                new LenientNominationForecastCreatedAtRangeRule(16, 120),
            });
    }
}
