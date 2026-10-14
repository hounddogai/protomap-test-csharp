namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy span summary must pass. Check returns its problems, or none.</summary>
public interface ILegacySpanSummaryRule
{
    List<string> Check(LegacySpanSummary item);
}

public sealed record LegacySpanSummaryOwnerIdLengthRule(int MaxLength) : ILegacySpanSummaryRule
{
    public List<string> Check(LegacySpanSummary item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanSummaryCodeLengthRule(int MaxLength) : ILegacySpanSummaryRule
{
    public List<string> Check(LegacySpanSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanSummaryJobCodeLengthRule(int MaxLength) : ILegacySpanSummaryRule
{
    public List<string> Check(LegacySpanSummary item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanSummaryTagsLimitRule(int MaxCount, List<string> Banned) : ILegacySpanSummaryRule
{
    public List<string> Check(LegacySpanSummary item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LegacySpanSummaryAllowedOwnerIdRule(List<string> Allowed) : ILegacySpanSummaryRule
{
    public List<string> Check(LegacySpanSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy span summary.</summary>
public sealed record LegacySpanSummaryValidator(List<ILegacySpanSummaryRule> Rules)
{
    public List<string> Validate(LegacySpanSummary item)
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

    public void RequireValid(LegacySpanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySpanSummary> items)
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

/// <summary>Functions of legacy span summary rules.</summary>
public static class LegacySpanSummaryRules
{
    public static LegacySpanSummaryValidator DefaultLegacySpanSummaryValidator()
    {
        return new LegacySpanSummaryValidator(
            new List<ILegacySpanSummaryRule>
            {
                new LegacySpanSummaryOwnerIdLengthRule(1000),
                new LegacySpanSummaryCodeLengthRule(64),
                new LegacySpanSummaryJobCodeLengthRule(16),
                new LegacySpanSummaryTagsLimitRule(86400, new List<string> { "lenient" }),
                new LegacySpanSummaryAllowedOwnerIdRule(new List<string> { "capped" }),
            });
    }
}
