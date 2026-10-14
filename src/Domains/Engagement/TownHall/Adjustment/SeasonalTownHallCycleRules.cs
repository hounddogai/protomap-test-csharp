namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal town hall cycle must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTownHallCycleRule
{
    List<string> Check(SeasonalTownHallCycle item);
}

public sealed record SeasonalTownHallCycleAllowedLabelRule(List<string> Allowed) : ISeasonalTownHallCycleRule
{
    public List<string> Check(SeasonalTownHallCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallCycleCodeLengthRule(int MaxLength) : ISeasonalTownHallCycleRule
{
    public List<string> Check(SeasonalTownHallCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallCycleRequiredCountryRule(string Message) : ISeasonalTownHallCycleRule
{
    public List<string> Check(SeasonalTownHallCycle item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallCycleCodePrefixRule(string Prefix) : ISeasonalTownHallCycleRule
{
    public List<string> Check(SeasonalTownHallCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal town hall cycle.</summary>
public sealed record SeasonalTownHallCycleValidator(List<ISeasonalTownHallCycleRule> Rules)
{
    public List<string> Validate(SeasonalTownHallCycle item)
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

    public void RequireValid(SeasonalTownHallCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTownHallCycle> items)
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

/// <summary>Functions of seasonal town hall cycle rules.</summary>
public static class SeasonalTownHallCycleRules
{
    public static SeasonalTownHallCycleValidator DefaultSeasonalTownHallCycleValidator()
    {
        return new SeasonalTownHallCycleValidator(
            new List<ISeasonalTownHallCycleRule>
            {
                new SeasonalTownHallCycleAllowedLabelRule(new List<string> { "default", "legacy" }),
                new SeasonalTownHallCycleCodeLengthRule(86400),
                new SeasonalTownHallCycleRequiredCountryRule("seasonal town hall cycle needs this field"),
                new SeasonalTownHallCycleCodePrefixRule("REF"),
            });
    }
}
