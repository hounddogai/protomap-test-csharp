namespace Acme.Workforce.Orgdesign.Span.Review;

/// <summary>A rule a regional span record must pass. Check returns its problems, or none.</summary>
public interface IRegionalSpanRecordRule
{
    List<string> Check(RegionalSpanRecord item);
}

public sealed record RegionalSpanRecordRequiredReviewerRule(string Message) : IRegionalSpanRecordRule
{
    public List<string> Check(RegionalSpanRecord item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanRecordRevisionRangeRule(int Min, int Max) : IRegionalSpanRecordRule
{
    public List<string> Check(RegionalSpanRecord item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanRecordBatchSizeRangeRule(int Min, int Max) : IRegionalSpanRecordRule
{
    public List<string> Check(RegionalSpanRecord item)
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

public sealed record RegionalSpanRecordRequiredTitleRule(string Message) : IRegionalSpanRecordRule
{
    public List<string> Check(RegionalSpanRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanRecordTitleLengthRule(int MaxLength) : IRegionalSpanRecordRule
{
    public List<string> Check(RegionalSpanRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional span record.</summary>
public sealed record RegionalSpanRecordValidator(List<IRegionalSpanRecordRule> Rules)
{
    public List<string> Validate(RegionalSpanRecord item)
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

    public void RequireValid(RegionalSpanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSpanRecord> items)
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

/// <summary>Functions of regional span record rules.</summary>
public static class RegionalSpanRecordRules
{
    public static RegionalSpanRecordValidator DefaultRegionalSpanRecordValidator()
    {
        return new RegionalSpanRecordValidator(
            new List<IRegionalSpanRecordRule>
            {
                new RegionalSpanRecordRequiredReviewerRule("regional span record needs this field"),
                new RegionalSpanRecordRevisionRangeRule(3600, 86400),
                new RegionalSpanRecordBatchSizeRangeRule(16, 1000),
                new RegionalSpanRecordRequiredTitleRule("regional span record needs a value"),
                new RegionalSpanRecordTitleLengthRule(32),
            });
    }
}
