namespace Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>A rule a legacy recognition cycle must pass. Check returns its problems, or none.</summary>
public interface ILegacyRecognitionCycleRule
{
    List<string> Check(LegacyRecognitionCycle item);
}

public sealed record LegacyRecognitionCycleArchivedGuardRule(double Limit) : ILegacyRecognitionCycleRule
{
    public List<string> Check(LegacyRecognitionCycle item)
    {
        if (item.Archived && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionCycleGradeLengthRule(int MaxLength) : ILegacyRecognitionCycleRule
{
    public List<string> Check(LegacyRecognitionCycle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionCycleCodePrefixRule(string Prefix) : ILegacyRecognitionCycleRule
{
    public List<string> Check(LegacyRecognitionCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionCycleHoursRangeRule(double Min, double Max) : ILegacyRecognitionCycleRule
{
    public List<string> Check(LegacyRecognitionCycle item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy recognition cycle.</summary>
public sealed record LegacyRecognitionCycleValidator(List<ILegacyRecognitionCycleRule> Rules)
{
    public List<string> Validate(LegacyRecognitionCycle item)
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

    public void RequireValid(LegacyRecognitionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRecognitionCycle> items)
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

/// <summary>Functions of legacy recognition cycle rules.</summary>
public static class LegacyRecognitionCycleRules
{
    public static LegacyRecognitionCycleValidator DefaultLegacyRecognitionCycleValidator()
    {
        return new LegacyRecognitionCycleValidator(
            new List<ILegacyRecognitionCycleRule>
            {
                new LegacyRecognitionCycleArchivedGuardRule(6.6),
                new LegacyRecognitionCycleGradeLengthRule(80),
                new LegacyRecognitionCycleCodePrefixRule("X"),
                new LegacyRecognitionCycleHoursRangeRule(9.4, 2838.9),
            });
    }
}
