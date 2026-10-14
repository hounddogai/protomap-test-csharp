namespace Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>A rule a visitor record must pass. Check returns its problems, or none.</summary>
public interface IVisitorRecordRule
{
    List<string> Check(VisitorRecord item);
}

public sealed record VisitorRecordRegionLengthRule(int MaxLength) : IVisitorRecordRule
{
    public List<string> Check(VisitorRecord item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRecordBadgeLengthRule(int MaxLength) : IVisitorRecordRule
{
    public List<string> Check(VisitorRecord item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRecordRequiredBadgeRule(string Message) : IVisitorRecordRule
{
    public List<string> Check(VisitorRecord item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor record.</summary>
public sealed record VisitorRecordValidator(List<IVisitorRecordRule> Rules)
{
    public List<string> Validate(VisitorRecord item)
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

    public void RequireValid(VisitorRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorRecord> items)
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

/// <summary>Functions of visitor record rules.</summary>
public static class VisitorRecordRules
{
    public static VisitorRecordValidator DefaultVisitorRecordValidator()
    {
        return new VisitorRecordValidator(
            new List<IVisitorRecordRule>
            {
                new VisitorRecordRegionLengthRule(1000),
                new VisitorRecordBadgeLengthRule(16),
                new VisitorRecordRequiredBadgeRule("visitor record needs an entry"),
            });
    }
}
