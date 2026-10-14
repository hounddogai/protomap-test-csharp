namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>A rule a global span adjustment must pass. Check returns its problems, or none.</summary>
public interface IGlobalSpanAdjustmentRule
{
    List<string> Check(GlobalSpanAdjustment item);
}

public sealed record GlobalSpanAdjustmentRequiredDescriptionRule(string Message) : IGlobalSpanAdjustmentRule
{
    public List<string> Check(GlobalSpanAdjustment item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanAdjustmentTagsLimitRule(int MaxCount, List<string> Banned) : IGlobalSpanAdjustmentRule
{
    public List<string> Check(GlobalSpanAdjustment item)
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

public sealed record GlobalSpanAdjustmentTitleLengthRule(int MaxLength) : IGlobalSpanAdjustmentRule
{
    public List<string> Check(GlobalSpanAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanAdjustmentCodeLengthRule(int MaxLength) : IGlobalSpanAdjustmentRule
{
    public List<string> Check(GlobalSpanAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanAdjustmentDescriptionLengthRule(int MaxLength) : IGlobalSpanAdjustmentRule
{
    public List<string> Check(GlobalSpanAdjustment item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global span adjustment.</summary>
public sealed record GlobalSpanAdjustmentValidator(List<IGlobalSpanAdjustmentRule> Rules)
{
    public List<string> Validate(GlobalSpanAdjustment item)
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

    public void RequireValid(GlobalSpanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalSpanAdjustment> items)
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

/// <summary>Functions of global span adjustment rules.</summary>
public static class GlobalSpanAdjustmentRules
{
    public static GlobalSpanAdjustmentValidator DefaultGlobalSpanAdjustmentValidator()
    {
        return new GlobalSpanAdjustmentValidator(
            new List<IGlobalSpanAdjustmentRule>
            {
                new GlobalSpanAdjustmentRequiredDescriptionRule("global span adjustment needs an entry"),
                new GlobalSpanAdjustmentTagsLimitRule(64, new List<string> { "weighted", "batched" }),
                new GlobalSpanAdjustmentTitleLengthRule(32),
                new GlobalSpanAdjustmentCodeLengthRule(120),
                new GlobalSpanAdjustmentDescriptionLengthRule(86400),
            });
    }
}
