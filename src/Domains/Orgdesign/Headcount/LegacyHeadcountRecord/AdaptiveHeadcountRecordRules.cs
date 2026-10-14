namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>A rule a adaptive headcount record must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveHeadcountRecordRule
{
    List<string> Check(AdaptiveHeadcountRecord item);
}

public sealed record AdaptiveHeadcountRecordRequiredJobCodeRule(string Message) : IAdaptiveHeadcountRecordRule
{
    public List<string> Check(AdaptiveHeadcountRecord item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountRecordBatchSizeRangeRule(int Min, int Max) : IAdaptiveHeadcountRecordRule
{
    public List<string> Check(AdaptiveHeadcountRecord item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveHeadcountRecordLabelLengthRule(int MaxLength) : IAdaptiveHeadcountRecordRule
{
    public List<string> Check(AdaptiveHeadcountRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountRecordRegionLengthRule(int MaxLength) : IAdaptiveHeadcountRecordRule
{
    public List<string> Check(AdaptiveHeadcountRecord item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive headcount record.</summary>
public sealed record AdaptiveHeadcountRecordValidator(List<IAdaptiveHeadcountRecordRule> Rules)
{
    public List<string> Validate(AdaptiveHeadcountRecord item)
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

    public void RequireValid(AdaptiveHeadcountRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveHeadcountRecord> items)
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

/// <summary>Functions of adaptive headcount record rules.</summary>
public static class AdaptiveHeadcountRecordRules
{
    public static AdaptiveHeadcountRecordValidator DefaultAdaptiveHeadcountRecordValidator()
    {
        return new AdaptiveHeadcountRecordValidator(
            new List<IAdaptiveHeadcountRecordRule>
            {
                new AdaptiveHeadcountRecordRequiredJobCodeRule("adaptive headcount record needs a value"),
                new AdaptiveHeadcountRecordBatchSizeRangeRule(64, 255),
                new AdaptiveHeadcountRecordLabelLengthRule(255),
                new AdaptiveHeadcountRecordRegionLengthRule(16),
            });
    }
}
