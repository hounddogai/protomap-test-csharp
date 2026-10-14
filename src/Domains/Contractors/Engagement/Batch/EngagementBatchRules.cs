namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement batch must pass. Check returns its problems, or none.</summary>
public interface IEngagementBatchRule
{
    List<string> Check(EngagementBatch item);
}

public sealed record EngagementBatchCapacityRangeRule(int Min, int Max) : IEngagementBatchRule
{
    public List<string> Check(EngagementBatch item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBatchLabelLengthRule(int MaxLength) : IEngagementBatchRule
{
    public List<string> Check(EngagementBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBatchRequiredLabelRule(string Message) : IEngagementBatchRule
{
    public List<string> Check(EngagementBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementBatchAllowedCodeRule(List<string> Allowed) : IEngagementBatchRule
{
    public List<string> Check(EngagementBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record EngagementBatchFteRangeRule(double Min, double Max) : IEngagementBatchRule
{
    public List<string> Check(EngagementBatch item)
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

/// <summary>Runs every rule of a engagement batch.</summary>
public sealed record EngagementBatchValidator(List<IEngagementBatchRule> Rules)
{
    public List<string> Validate(EngagementBatch item)
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

    public void RequireValid(EngagementBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementBatch> items)
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

/// <summary>Functions of engagement batch rules.</summary>
public static class EngagementBatchRules
{
    public static EngagementBatchValidator DefaultEngagementBatchValidator()
    {
        return new EngagementBatchValidator(
            new List<IEngagementBatchRule>
            {
                new EngagementBatchCapacityRangeRule(64, 80),
                new EngagementBatchLabelLengthRule(16),
                new EngagementBatchRequiredLabelRule("engagement batch needs a value"),
                new EngagementBatchAllowedCodeRule(new List<string>()),
                new EngagementBatchFteRangeRule(2.0, 3151.9),
            });
    }
}
