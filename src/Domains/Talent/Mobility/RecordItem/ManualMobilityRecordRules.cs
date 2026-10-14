namespace Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>A rule a manual mobility record must pass. Check returns its problems, or none.</summary>
public interface IManualMobilityRecordRule
{
    List<string> Check(ManualMobilityRecord item);
}

public sealed record ManualMobilityRecordCodeLengthRule(int MaxLength) : IManualMobilityRecordRule
{
    public List<string> Check(ManualMobilityRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityRecordTitleLengthRule(int MaxLength) : IManualMobilityRecordRule
{
    public List<string> Check(ManualMobilityRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityRecordRequiredTitleRule(string Message) : IManualMobilityRecordRule
{
    public List<string> Check(ManualMobilityRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityRecordTimezoneLengthRule(int MaxLength) : IManualMobilityRecordRule
{
    public List<string> Check(ManualMobilityRecord item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual mobility record.</summary>
public sealed record ManualMobilityRecordValidator(List<IManualMobilityRecordRule> Rules)
{
    public List<string> Validate(ManualMobilityRecord item)
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

    public void RequireValid(ManualMobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualMobilityRecord> items)
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

/// <summary>Functions of manual mobility record rules.</summary>
public static class ManualMobilityRecordRules
{
    public static ManualMobilityRecordValidator DefaultManualMobilityRecordValidator()
    {
        return new ManualMobilityRecordValidator(
            new List<IManualMobilityRecordRule>
            {
                new ManualMobilityRecordCodeLengthRule(86400),
                new ManualMobilityRecordTitleLengthRule(1000),
                new ManualMobilityRecordRequiredTitleRule("manual mobility record needs this field"),
                new ManualMobilityRecordTimezoneLengthRule(86400),
            });
    }
}
