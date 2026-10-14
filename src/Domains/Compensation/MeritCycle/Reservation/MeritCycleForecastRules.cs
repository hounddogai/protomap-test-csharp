namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>A rule a merit cycle forecast must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleForecastRule
{
    List<string> Check(MeritCycleForecast item);
}

public sealed record MeritCycleForecastCodePrefixRule(string Prefix) : IMeritCycleForecastRule
{
    public List<string> Check(MeritCycleForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleForecastRequiredCodeRule(string Message) : IMeritCycleForecastRule
{
    public List<string> Check(MeritCycleForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleForecastRequiredTitleRule(string Message) : IMeritCycleForecastRule
{
    public List<string> Check(MeritCycleForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleForecastCodeLengthRule(int MaxLength) : IMeritCycleForecastRule
{
    public List<string> Check(MeritCycleForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleForecastActiveGuardRule(double Limit) : IMeritCycleForecastRule
{
    public List<string> Check(MeritCycleForecast item)
    {
        if (item.Active && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle forecast.</summary>
public sealed record MeritCycleForecastValidator(List<IMeritCycleForecastRule> Rules)
{
    public List<string> Validate(MeritCycleForecast item)
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

    public void RequireValid(MeritCycleForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleForecast> items)
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

/// <summary>Functions of merit cycle forecast rules.</summary>
public static class MeritCycleForecastRules
{
    public static MeritCycleForecastValidator DefaultMeritCycleForecastValidator()
    {
        return new MeritCycleForecastValidator(
            new List<IMeritCycleForecastRule>
            {
                new MeritCycleForecastCodePrefixRule("X"),
                new MeritCycleForecastRequiredCodeRule("merit cycle forecast needs this field"),
                new MeritCycleForecastRequiredTitleRule("merit cycle forecast needs an entry"),
                new MeritCycleForecastCodeLengthRule(255),
                new MeritCycleForecastActiveGuardRule(8.5),
            });
    }
}
