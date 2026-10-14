namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>A rule a seasonal license seat adjustment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLicenseSeatAdjustmentRule
{
    List<string> Check(SeasonalLicenseSeatAdjustment item);
}

public sealed record SeasonalLicenseSeatAdjustmentCodePrefixRule(string Prefix) : ISeasonalLicenseSeatAdjustmentRule
{
    public List<string> Check(SeasonalLicenseSeatAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatAdjustmentRequiredTitleRule(string Message) : ISeasonalLicenseSeatAdjustmentRule
{
    public List<string> Check(SeasonalLicenseSeatAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatAdjustmentTitleLengthRule(int MaxLength) : ISeasonalLicenseSeatAdjustmentRule
{
    public List<string> Check(SeasonalLicenseSeatAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatAdjustmentMarginRangeRule(double Min, double Max) : ISeasonalLicenseSeatAdjustmentRule
{
    public List<string> Check(SeasonalLicenseSeatAdjustment item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatAdjustmentCodeLengthRule(int MaxLength) : ISeasonalLicenseSeatAdjustmentRule
{
    public List<string> Check(SeasonalLicenseSeatAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal license seat adjustment.</summary>
public sealed record SeasonalLicenseSeatAdjustmentValidator(List<ISeasonalLicenseSeatAdjustmentRule> Rules)
{
    public List<string> Validate(SeasonalLicenseSeatAdjustment item)
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

    public void RequireValid(SeasonalLicenseSeatAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLicenseSeatAdjustment> items)
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

/// <summary>Functions of seasonal license seat adjustment rules.</summary>
public static class SeasonalLicenseSeatAdjustmentRules
{
    public static SeasonalLicenseSeatAdjustmentValidator DefaultSeasonalLicenseSeatAdjustmentValidator()
    {
        return new SeasonalLicenseSeatAdjustmentValidator(
            new List<ISeasonalLicenseSeatAdjustmentRule>
            {
                new SeasonalLicenseSeatAdjustmentCodePrefixRule("REF"),
                new SeasonalLicenseSeatAdjustmentRequiredTitleRule("seasonal license seat adjustment needs a value"),
                new SeasonalLicenseSeatAdjustmentTitleLengthRule(255),
                new SeasonalLicenseSeatAdjustmentMarginRangeRule(7.7, 3511.9),
                new SeasonalLicenseSeatAdjustmentCodeLengthRule(86400),
            });
    }
}
