namespace Acme.Workforce.Talent.Nomination.Profile;

/// <summary>A rule a seasonal nomination cycle must pass. Check returns its problems, or none.</summary>
public interface ISeasonalNominationCycleRule
{
    List<string> Check(SeasonalNominationCycle item);
}

public sealed record SeasonalNominationCycleCodeLengthRule(int MaxLength) : ISeasonalNominationCycleRule
{
    public List<string> Check(SeasonalNominationCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalNominationCycleFteRangeRule(double Min, double Max) : ISeasonalNominationCycleRule
{
    public List<string> Check(SeasonalNominationCycle item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalNominationCycleCodePrefixRule(string Prefix) : ISeasonalNominationCycleRule
{
    public List<string> Check(SeasonalNominationCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal nomination cycle.</summary>
public sealed record SeasonalNominationCycleValidator(List<ISeasonalNominationCycleRule> Rules)
{
    public List<string> Validate(SeasonalNominationCycle item)
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

    public void RequireValid(SeasonalNominationCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalNominationCycle> items)
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

/// <summary>Functions of seasonal nomination cycle rules.</summary>
public static class SeasonalNominationCycleRules
{
    public static SeasonalNominationCycleValidator DefaultSeasonalNominationCycleValidator()
    {
        return new SeasonalNominationCycleValidator(
            new List<ISeasonalNominationCycleRule>
            {
                new SeasonalNominationCycleCodeLengthRule(32),
                new SeasonalNominationCycleFteRangeRule(9.0, 1499.2),
                new SeasonalNominationCycleCodePrefixRule("Q"),
            });
    }
}
