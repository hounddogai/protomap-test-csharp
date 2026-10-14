namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>A rule a seasonal license seat preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLicenseSeatPreferenceRule
{
    List<string> Check(SeasonalLicenseSeatPreference item);
}

public sealed record SeasonalLicenseSeatPreferenceRequiredNameRule(string Message) : ISeasonalLicenseSeatPreferenceRule
{
    public List<string> Check(SeasonalLicenseSeatPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLicenseSeatPreferenceQuantityRangeRule(int Min, int Max) : ISeasonalLicenseSeatPreferenceRule
{
    public List<string> Check(SeasonalLicenseSeatPreference item)
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

public sealed record SeasonalLicenseSeatPreferenceRequiredCodeRule(string Message) : ISeasonalLicenseSeatPreferenceRule
{
    public List<string> Check(SeasonalLicenseSeatPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal license seat preference.</summary>
public sealed record SeasonalLicenseSeatPreferenceValidator(List<ISeasonalLicenseSeatPreferenceRule> Rules)
{
    public List<string> Validate(SeasonalLicenseSeatPreference item)
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

    public void RequireValid(SeasonalLicenseSeatPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLicenseSeatPreference> items)
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

/// <summary>Functions of seasonal license seat preference rules.</summary>
public static class SeasonalLicenseSeatPreferenceRules
{
    public static SeasonalLicenseSeatPreferenceValidator DefaultSeasonalLicenseSeatPreferenceValidator()
    {
        return new SeasonalLicenseSeatPreferenceValidator(
            new List<ISeasonalLicenseSeatPreferenceRule>
            {
                new SeasonalLicenseSeatPreferenceRequiredNameRule("seasonal license seat preference needs an entry"),
                new SeasonalLicenseSeatPreferenceQuantityRangeRule(120, 86400),
                new SeasonalLicenseSeatPreferenceRequiredCodeRule("seasonal license seat preference needs an entry"),
            });
    }
}
