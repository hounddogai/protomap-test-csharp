namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>A rule a seasonal license seat summary must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLicenseSeatSummaryRule
{
    List<string> Check(SeasonalLicenseSeatSummary item);
}

public sealed record SeasonalLicenseSeatSummaryCodePrefixRule(string Prefix) : ISeasonalLicenseSeatSummaryRule
{
    public List<string> Check(SeasonalLicenseSeatSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatSummaryCodeLengthRule(int MaxLength) : ISeasonalLicenseSeatSummaryRule
{
    public List<string> Check(SeasonalLicenseSeatSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatSummaryRequiredCodeRule(string Message) : ISeasonalLicenseSeatSummaryRule
{
    public List<string> Check(SeasonalLicenseSeatSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal license seat summary.</summary>
public sealed record SeasonalLicenseSeatSummaryValidator(List<ISeasonalLicenseSeatSummaryRule> Rules)
{
    public List<string> Validate(SeasonalLicenseSeatSummary item)
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

    public void RequireValid(SeasonalLicenseSeatSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLicenseSeatSummary> items)
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

/// <summary>Functions of seasonal license seat summary rules.</summary>
public static class SeasonalLicenseSeatSummaryRules
{
    public static SeasonalLicenseSeatSummaryValidator DefaultSeasonalLicenseSeatSummaryValidator()
    {
        return new SeasonalLicenseSeatSummaryValidator(
            new List<ISeasonalLicenseSeatSummaryRule>
            {
                new SeasonalLicenseSeatSummaryCodePrefixRule("OPS"),
                new SeasonalLicenseSeatSummaryCodeLengthRule(86400),
                new SeasonalLicenseSeatSummaryRequiredCodeRule("seasonal license seat summary needs a value"),
            });
    }
}
