namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>A rule a rolling license seat ledger must pass. Check returns its problems, or none.</summary>
public interface IRollingLicenseSeatLedgerRule
{
    List<string> Check(RollingLicenseSeatLedger item);
}

public sealed record RollingLicenseSeatLedgerRequiredCodeRule(string Message) : IRollingLicenseSeatLedgerRule
{
    public List<string> Check(RollingLicenseSeatLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLicenseSeatLedgerCodePrefixRule(string Prefix) : IRollingLicenseSeatLedgerRule
{
    public List<string> Check(RollingLicenseSeatLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLicenseSeatLedgerRatioRangeRule(double Min, double Max) : IRollingLicenseSeatLedgerRule
{
    public List<string> Check(RollingLicenseSeatLedger item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling license seat ledger.</summary>
public sealed record RollingLicenseSeatLedgerValidator(List<IRollingLicenseSeatLedgerRule> Rules)
{
    public List<string> Validate(RollingLicenseSeatLedger item)
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

    public void RequireValid(RollingLicenseSeatLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLicenseSeatLedger> items)
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

/// <summary>Functions of rolling license seat ledger rules.</summary>
public static class RollingLicenseSeatLedgerRules
{
    public static RollingLicenseSeatLedgerValidator DefaultRollingLicenseSeatLedgerValidator()
    {
        return new RollingLicenseSeatLedgerValidator(
            new List<IRollingLicenseSeatLedgerRule>
            {
                new RollingLicenseSeatLedgerRequiredCodeRule("rolling license seat ledger needs an entry"),
                new RollingLicenseSeatLedgerCodePrefixRule("REF"),
                new RollingLicenseSeatLedgerRatioRangeRule(0.9, 3564.3),
            });
    }
}
