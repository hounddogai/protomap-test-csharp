namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>A rule a license seat forecast must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatForecastRule
{
    List<string> Check(LicenseSeatForecast item);
}

public sealed record LicenseSeatForecastNameLengthRule(int MaxLength) : ILicenseSeatForecastRule
{
    public List<string> Check(LicenseSeatForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatForecastExpiresAtRangeRule(int Min, int Max) : ILicenseSeatForecastRule
{
    public List<string> Check(LicenseSeatForecast item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatForecastRequiredChecksumRule(string Message) : ILicenseSeatForecastRule
{
    public List<string> Check(LicenseSeatForecast item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatForecastRequiredNameRule(string Message) : ILicenseSeatForecastRule
{
    public List<string> Check(LicenseSeatForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatForecastRequiredDescriptionRule(string Message) : ILicenseSeatForecastRule
{
    public List<string> Check(LicenseSeatForecast item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat forecast.</summary>
public sealed record LicenseSeatForecastValidator(List<ILicenseSeatForecastRule> Rules)
{
    public List<string> Validate(LicenseSeatForecast item)
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

    public void RequireValid(LicenseSeatForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatForecast> items)
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

/// <summary>Functions of license seat forecast rules.</summary>
public static class LicenseSeatForecastRules
{
    public static LicenseSeatForecastValidator DefaultLicenseSeatForecastValidator()
    {
        return new LicenseSeatForecastValidator(
            new List<ILicenseSeatForecastRule>
            {
                new LicenseSeatForecastNameLengthRule(80),
                new LicenseSeatForecastExpiresAtRangeRule(64, 86400),
                new LicenseSeatForecastRequiredChecksumRule("license seat forecast needs a value"),
                new LicenseSeatForecastRequiredNameRule("license seat forecast needs this field"),
                new LicenseSeatForecastRequiredDescriptionRule("license seat forecast needs a value"),
            });
    }
}
