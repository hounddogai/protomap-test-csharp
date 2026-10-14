namespace Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>A rule a default mobility record must pass. Check returns its problems, or none.</summary>
public interface IDefaultMobilityRecordRule
{
    List<string> Check(DefaultMobilityRecord item);
}

public sealed record DefaultMobilityRecordRatioRangeRule(double Min, double Max) : IDefaultMobilityRecordRule
{
    public List<string> Check(DefaultMobilityRecord item)
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

public sealed record DefaultMobilityRecordRequiredCodeRule(string Message) : IDefaultMobilityRecordRule
{
    public List<string> Check(DefaultMobilityRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityRecordWindowDaysRangeRule(int Min, int Max) : IDefaultMobilityRecordRule
{
    public List<string> Check(DefaultMobilityRecord item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultMobilityRecordRequiredSegmentRule(string Message) : IDefaultMobilityRecordRule
{
    public List<string> Check(DefaultMobilityRecord item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default mobility record.</summary>
public sealed record DefaultMobilityRecordValidator(List<IDefaultMobilityRecordRule> Rules)
{
    public List<string> Validate(DefaultMobilityRecord item)
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

    public void RequireValid(DefaultMobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultMobilityRecord> items)
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

/// <summary>Functions of default mobility record rules.</summary>
public static class DefaultMobilityRecordRules
{
    public static DefaultMobilityRecordValidator DefaultDefaultMobilityRecordValidator()
    {
        return new DefaultMobilityRecordValidator(
            new List<IDefaultMobilityRecordRule>
            {
                new DefaultMobilityRecordRatioRangeRule(1.5, 2718.0),
                new DefaultMobilityRecordRequiredCodeRule("default mobility record needs an entry"),
                new DefaultMobilityRecordWindowDaysRangeRule(1000, 86400),
                new DefaultMobilityRecordRequiredSegmentRule("default mobility record needs this field"),
            });
    }
}
