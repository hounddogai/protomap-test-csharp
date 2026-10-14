namespace Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>A rule a seasonal cost model preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCostModelPreferenceRule
{
    List<string> Check(SeasonalCostModelPreference item);
}

public sealed record SeasonalCostModelPreferenceTeamLengthRule(int MaxLength) : ISeasonalCostModelPreferenceRule
{
    public List<string> Check(SeasonalCostModelPreference item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelPreferenceCodeLengthRule(int MaxLength) : ISeasonalCostModelPreferenceRule
{
    public List<string> Check(SeasonalCostModelPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelPreferenceCountryLengthRule(int MaxLength) : ISeasonalCostModelPreferenceRule
{
    public List<string> Check(SeasonalCostModelPreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelPreferenceRequiredTeamRule(string Message) : ISeasonalCostModelPreferenceRule
{
    public List<string> Check(SeasonalCostModelPreference item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal cost model preference.</summary>
public sealed record SeasonalCostModelPreferenceValidator(List<ISeasonalCostModelPreferenceRule> Rules)
{
    public List<string> Validate(SeasonalCostModelPreference item)
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

    public void RequireValid(SeasonalCostModelPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCostModelPreference> items)
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

/// <summary>Functions of seasonal cost model preference rules.</summary>
public static class SeasonalCostModelPreferenceRules
{
    public static SeasonalCostModelPreferenceValidator DefaultSeasonalCostModelPreferenceValidator()
    {
        return new SeasonalCostModelPreferenceValidator(
            new List<ISeasonalCostModelPreferenceRule>
            {
                new SeasonalCostModelPreferenceTeamLengthRule(16),
                new SeasonalCostModelPreferenceCodeLengthRule(120),
                new SeasonalCostModelPreferenceCountryLengthRule(3600),
                new SeasonalCostModelPreferenceRequiredTeamRule("seasonal cost model preference needs this field"),
            });
    }
}
