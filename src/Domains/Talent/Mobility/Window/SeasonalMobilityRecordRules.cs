namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal mobility record must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMobilityRecordRule
{
    List<string> Check(SeasonalMobilityRecord item);
}

public sealed record SeasonalMobilityRecordCodePrefixRule(string Prefix) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityRecordRequiredGradeRule(string Message) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityRecordStartsAtRangeRule(int Min, int Max) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityRecordCodeLengthRule(int MaxLength) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityRecordAllowedRegionRule(List<string> Allowed) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityRecordScoreRangeRule(double Min, double Max) : ISeasonalMobilityRecordRule
{
    public List<string> Check(SeasonalMobilityRecord item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal mobility record.</summary>
public sealed record SeasonalMobilityRecordValidator(List<ISeasonalMobilityRecordRule> Rules)
{
    public List<string> Validate(SeasonalMobilityRecord item)
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

    public void RequireValid(SeasonalMobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMobilityRecord> items)
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

/// <summary>Functions of seasonal mobility record rules.</summary>
public static class SeasonalMobilityRecordRules
{
    public static SeasonalMobilityRecordValidator DefaultSeasonalMobilityRecordValidator()
    {
        return new SeasonalMobilityRecordValidator(
            new List<ISeasonalMobilityRecordRule>
            {
                new SeasonalMobilityRecordCodePrefixRule("ACM"),
                new SeasonalMobilityRecordRequiredGradeRule("seasonal mobility record needs a value"),
                new SeasonalMobilityRecordStartsAtRangeRule(16, 255),
                new SeasonalMobilityRecordCodeLengthRule(80),
                new SeasonalMobilityRecordAllowedRegionRule(new List<string> { "strict", "tiered", "primary" }),
                new SeasonalMobilityRecordScoreRangeRule(5.2, 4863.6),
            });
    }
}
