namespace Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>A rule a capped desk record must pass. Check returns its problems, or none.</summary>
public interface ICappedDeskRecordRule
{
    List<string> Check(CappedDeskRecord item);
}

public sealed record CappedDeskRecordRequiredCodeRule(string Message) : ICappedDeskRecordRule
{
    public List<string> Check(CappedDeskRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskRecordCodePrefixRule(string Prefix) : ICappedDeskRecordRule
{
    public List<string> Check(CappedDeskRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskRecordRequiredNameRule(string Message) : ICappedDeskRecordRule
{
    public List<string> Check(CappedDeskRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskRecordRequiredSegmentRule(string Message) : ICappedDeskRecordRule
{
    public List<string> Check(CappedDeskRecord item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped desk record.</summary>
public sealed record CappedDeskRecordValidator(List<ICappedDeskRecordRule> Rules)
{
    public List<string> Validate(CappedDeskRecord item)
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

    public void RequireValid(CappedDeskRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeskRecord> items)
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

/// <summary>Functions of capped desk record rules.</summary>
public static class CappedDeskRecordRules
{
    public static CappedDeskRecordValidator DefaultCappedDeskRecordValidator()
    {
        return new CappedDeskRecordValidator(
            new List<ICappedDeskRecordRule>
            {
                new CappedDeskRecordRequiredCodeRule("capped desk record needs an entry"),
                new CappedDeskRecordCodePrefixRule("OPS"),
                new CappedDeskRecordRequiredNameRule("capped desk record needs an entry"),
                new CappedDeskRecordRequiredSegmentRule("capped desk record needs an entry"),
            });
    }
}
