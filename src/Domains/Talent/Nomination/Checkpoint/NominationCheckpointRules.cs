namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination checkpoint must pass. Check returns its problems, or none.</summary>
public interface INominationCheckpointRule
{
    List<string> Check(NominationCheckpoint item);
}

public sealed record NominationCheckpointTagsLimitRule(int MaxCount, List<string> Banned) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
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

public sealed record NominationCheckpointRequiredUnitRule(string Message) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationCheckpointTitleLengthRule(int MaxLength) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationCheckpointCapacityRangeRule(int Min, int Max) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationCheckpointAllowedCategoryRule(List<string> Allowed) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

public sealed record NominationCheckpointCategoryLengthRule(int MaxLength) : INominationCheckpointRule
{
    public List<string> Check(NominationCheckpoint item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination checkpoint.</summary>
public sealed record NominationCheckpointValidator(List<INominationCheckpointRule> Rules)
{
    public List<string> Validate(NominationCheckpoint item)
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

    public void RequireValid(NominationCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationCheckpoint> items)
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

/// <summary>Functions of nomination checkpoint rules.</summary>
public static class NominationCheckpointRules
{
    public static NominationCheckpointValidator DefaultNominationCheckpointValidator()
    {
        return new NominationCheckpointValidator(
            new List<INominationCheckpointRule>
            {
                new NominationCheckpointTagsLimitRule(1000, new List<string> { "manual", "scheduled", "batched" }),
                new NominationCheckpointRequiredUnitRule("nomination checkpoint needs an entry"),
                new NominationCheckpointTitleLengthRule(3600),
                new NominationCheckpointCapacityRangeRule(64, 255),
                new NominationCheckpointAllowedCategoryRule(new List<string>()),
                new NominationCheckpointCategoryLengthRule(80),
            });
    }
}
