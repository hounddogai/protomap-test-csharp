namespace Acme.Workforce.Talent.Mobility.Tier;

/// <summary>A rule a mobility record must pass. Check returns its problems, or none.</summary>
public interface IMobilityRecordRule
{
    List<string> Check(MobilityRecord item);
}

public sealed record MobilityRecordTimezoneLengthRule(int MaxLength) : IMobilityRecordRule
{
    public List<string> Check(MobilityRecord item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRecordRequiredTimezoneRule(string Message) : IMobilityRecordRule
{
    public List<string> Check(MobilityRecord item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityRecordScoreRangeRule(double Min, double Max) : IMobilityRecordRule
{
    public List<string> Check(MobilityRecord item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a mobility record.</summary>
public sealed record MobilityRecordValidator(List<IMobilityRecordRule> Rules)
{
    public List<string> Validate(MobilityRecord item)
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

    public void RequireValid(MobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityRecord> items)
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

/// <summary>Functions of mobility record rules.</summary>
public static class MobilityRecordRules
{
    public static MobilityRecordValidator DefaultMobilityRecordValidator()
    {
        return new MobilityRecordValidator(
            new List<IMobilityRecordRule>
            {
                new MobilityRecordTimezoneLengthRule(16),
                new MobilityRecordRequiredTimezoneRule("mobility record needs this field"),
                new MobilityRecordScoreRangeRule(7.7, 3842.2),
            });
    }
}
