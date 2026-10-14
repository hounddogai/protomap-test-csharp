namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>A rule a seasonal license seat window must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLicenseSeatWindowRule
{
    List<string> Check(SeasonalLicenseSeatWindow item);
}

public sealed record SeasonalLicenseSeatWindowRatioRangeRule(double Min, double Max) : ISeasonalLicenseSeatWindowRule
{
    public List<string> Check(SeasonalLicenseSeatWindow item)
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

public sealed record SeasonalLicenseSeatWindowCodePrefixRule(string Prefix) : ISeasonalLicenseSeatWindowRule
{
    public List<string> Check(SeasonalLicenseSeatWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatWindowRequiredCodeRule(string Message) : ISeasonalLicenseSeatWindowRule
{
    public List<string> Check(SeasonalLicenseSeatWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatWindowTitleLengthRule(int MaxLength) : ISeasonalLicenseSeatWindowRule
{
    public List<string> Check(SeasonalLicenseSeatWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatWindowCodeLengthRule(int MaxLength) : ISeasonalLicenseSeatWindowRule
{
    public List<string> Check(SeasonalLicenseSeatWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal license seat window.</summary>
public sealed record SeasonalLicenseSeatWindowValidator(List<ISeasonalLicenseSeatWindowRule> Rules)
{
    public List<string> Validate(SeasonalLicenseSeatWindow item)
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

    public void RequireValid(SeasonalLicenseSeatWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLicenseSeatWindow> items)
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

/// <summary>Functions of seasonal license seat window rules.</summary>
public static class SeasonalLicenseSeatWindowRules
{
    public static SeasonalLicenseSeatWindowValidator DefaultSeasonalLicenseSeatWindowValidator()
    {
        return new SeasonalLicenseSeatWindowValidator(
            new List<ISeasonalLicenseSeatWindowRule>
            {
                new SeasonalLicenseSeatWindowRatioRangeRule(7.7, 1879.2),
                new SeasonalLicenseSeatWindowCodePrefixRule("OPS"),
                new SeasonalLicenseSeatWindowRequiredCodeRule("seasonal license seat window needs a value"),
                new SeasonalLicenseSeatWindowTitleLengthRule(64),
                new SeasonalLicenseSeatWindowCodeLengthRule(64),
            });
    }
}
