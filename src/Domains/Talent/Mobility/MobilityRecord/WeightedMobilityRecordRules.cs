namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>A rule a weighted mobility record must pass. Check returns its problems, or none.</summary>
public interface IWeightedMobilityRecordRule
{
    List<string> Check(WeightedMobilityRecord item);
}

public sealed record WeightedMobilityRecordCodeLengthRule(int MaxLength) : IWeightedMobilityRecordRule
{
    public List<string> Check(WeightedMobilityRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMobilityRecordCeilingRangeRule(double Min, double Max) : IWeightedMobilityRecordRule
{
    public List<string> Check(WeightedMobilityRecord item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMobilityRecordNameLengthRule(int MaxLength) : IWeightedMobilityRecordRule
{
    public List<string> Check(WeightedMobilityRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMobilityRecordRequiredCodeRule(string Message) : IWeightedMobilityRecordRule
{
    public List<string> Check(WeightedMobilityRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedMobilityRecordRequiredNameRule(string Message) : IWeightedMobilityRecordRule
{
    public List<string> Check(WeightedMobilityRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted mobility record.</summary>
public sealed record WeightedMobilityRecordValidator(List<IWeightedMobilityRecordRule> Rules)
{
    public List<string> Validate(WeightedMobilityRecord item)
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

    public void RequireValid(WeightedMobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedMobilityRecord> items)
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

/// <summary>Functions of weighted mobility record rules.</summary>
public static class WeightedMobilityRecordRules
{
    public static WeightedMobilityRecordValidator DefaultWeightedMobilityRecordValidator()
    {
        return new WeightedMobilityRecordValidator(
            new List<IWeightedMobilityRecordRule>
            {
                new WeightedMobilityRecordCodeLengthRule(1000),
                new WeightedMobilityRecordCeilingRangeRule(5.4, 707.1),
                new WeightedMobilityRecordNameLengthRule(3600),
                new WeightedMobilityRecordRequiredCodeRule("weighted mobility record needs this field"),
                new WeightedMobilityRecordRequiredNameRule("weighted mobility record needs a value"),
            });
    }
}
