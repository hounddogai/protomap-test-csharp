namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending license seat forecast must pass. Check returns its problems, or none.</summary>
public interface IPendingLicenseSeatForecastRule
{
    List<string> Check(PendingLicenseSeatForecast item);
}

public sealed record PendingLicenseSeatForecastAllowedCostCenterRule(List<string> Allowed) : IPendingLicenseSeatForecastRule
{
    public List<string> Check(PendingLicenseSeatForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatForecastQuantityRangeRule(int Min, int Max) : IPendingLicenseSeatForecastRule
{
    public List<string> Check(PendingLicenseSeatForecast item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingLicenseSeatForecastCodeLengthRule(int MaxLength) : IPendingLicenseSeatForecastRule
{
    public List<string> Check(PendingLicenseSeatForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatForecastCodePrefixRule(string Prefix) : IPendingLicenseSeatForecastRule
{
    public List<string> Check(PendingLicenseSeatForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending license seat forecast.</summary>
public sealed record PendingLicenseSeatForecastValidator(List<IPendingLicenseSeatForecastRule> Rules)
{
    public List<string> Validate(PendingLicenseSeatForecast item)
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

    public void RequireValid(PendingLicenseSeatForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLicenseSeatForecast> items)
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

/// <summary>Functions of pending license seat forecast rules.</summary>
public static class PendingLicenseSeatForecastRules
{
    public static PendingLicenseSeatForecastValidator DefaultPendingLicenseSeatForecastValidator()
    {
        return new PendingLicenseSeatForecastValidator(
            new List<IPendingLicenseSeatForecastRule>
            {
                new PendingLicenseSeatForecastAllowedCostCenterRule(
                    new List<string> { "seasonal", "regional", "global" }),
                new PendingLicenseSeatForecastQuantityRangeRule(1000, 86400),
                new PendingLicenseSeatForecastCodeLengthRule(120),
                new PendingLicenseSeatForecastCodePrefixRule("ACM"),
            });
    }
}
