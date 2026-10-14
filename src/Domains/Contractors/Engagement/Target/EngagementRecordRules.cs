namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement record must pass. Check returns its problems, or none.</summary>
public interface IEngagementRecordRule
{
    List<string> Check(EngagementRecord item);
}

public sealed record EngagementRecordSummaryLengthRule(int MaxLength) : IEngagementRecordRule
{
    public List<string> Check(EngagementRecord item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRecordExternalRefLengthRule(int MaxLength) : IEngagementRecordRule
{
    public List<string> Check(EngagementRecord item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRecordFteRangeRule(double Min, double Max) : IEngagementRecordRule
{
    public List<string> Check(EngagementRecord item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementRecordAllowedTitleRule(List<string> Allowed) : IEngagementRecordRule
{
    public List<string> Check(EngagementRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement record.</summary>
public sealed record EngagementRecordValidator(List<IEngagementRecordRule> Rules)
{
    public List<string> Validate(EngagementRecord item)
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

    public void RequireValid(EngagementRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementRecord> items)
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

/// <summary>Functions of engagement record rules.</summary>
public static class EngagementRecordRules
{
    public static EngagementRecordValidator DefaultEngagementRecordValidator()
    {
        return new EngagementRecordValidator(
            new List<IEngagementRecordRule>
            {
                new EngagementRecordSummaryLengthRule(3600),
                new EngagementRecordExternalRefLengthRule(64),
                new EngagementRecordFteRangeRule(8.8, 4961.2),
                new EngagementRecordAllowedTitleRule(new List<string> { "strict", "primary" }),
            });
    }
}
