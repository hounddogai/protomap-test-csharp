namespace Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>A rule a legacy span record must pass. Check returns its problems, or none.</summary>
public interface ILegacySpanRecordRule
{
    List<string> Check(LegacySpanRecord item);
}

public sealed record LegacySpanRecordQuantityRangeRule(int Min, int Max) : ILegacySpanRecordRule
{
    public List<string> Check(LegacySpanRecord item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanRecordRegionLengthRule(int MaxLength) : ILegacySpanRecordRule
{
    public List<string> Check(LegacySpanRecord item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanRecordManagerIdLengthRule(int MaxLength) : ILegacySpanRecordRule
{
    public List<string> Check(LegacySpanRecord item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanRecordTitleLengthRule(int MaxLength) : ILegacySpanRecordRule
{
    public List<string> Check(LegacySpanRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy span record.</summary>
public sealed record LegacySpanRecordValidator(List<ILegacySpanRecordRule> Rules)
{
    public List<string> Validate(LegacySpanRecord item)
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

    public void RequireValid(LegacySpanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySpanRecord> items)
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

/// <summary>Functions of legacy span record rules.</summary>
public static class LegacySpanRecordRules
{
    public static LegacySpanRecordValidator DefaultLegacySpanRecordValidator()
    {
        return new LegacySpanRecordValidator(
            new List<ILegacySpanRecordRule>
            {
                new LegacySpanRecordQuantityRangeRule(80, 86400),
                new LegacySpanRecordRegionLengthRule(255),
                new LegacySpanRecordManagerIdLengthRule(1000),
                new LegacySpanRecordTitleLengthRule(255),
            });
    }
}
