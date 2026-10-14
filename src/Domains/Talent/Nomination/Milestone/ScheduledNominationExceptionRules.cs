namespace Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>A rule a scheduled nomination exception must pass. Check returns its problems, or none.</summary>
public interface IScheduledNominationExceptionRule
{
    List<string> Check(ScheduledNominationException item);
}

public sealed record ScheduledNominationExceptionRequiredManagerIdRule(string Message) : IScheduledNominationExceptionRule
{
    public List<string> Check(ScheduledNominationException item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledNominationExceptionRule
{
    public List<string> Check(ScheduledNominationException item)
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

public sealed record ScheduledNominationExceptionCodeLengthRule(int MaxLength) : IScheduledNominationExceptionRule
{
    public List<string> Check(ScheduledNominationException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationExceptionManagerIdLengthRule(int MaxLength) : IScheduledNominationExceptionRule
{
    public List<string> Check(ScheduledNominationException item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled nomination exception.</summary>
public sealed record ScheduledNominationExceptionValidator(List<IScheduledNominationExceptionRule> Rules)
{
    public List<string> Validate(ScheduledNominationException item)
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

    public void RequireValid(ScheduledNominationException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledNominationException> items)
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

/// <summary>Functions of scheduled nomination exception rules.</summary>
public static class ScheduledNominationExceptionRules
{
    public static ScheduledNominationExceptionValidator DefaultScheduledNominationExceptionValidator()
    {
        return new ScheduledNominationExceptionValidator(
            new List<IScheduledNominationExceptionRule>
            {
                new ScheduledNominationExceptionRequiredManagerIdRule(
                    "scheduled nomination exception needs this field"),
                new ScheduledNominationExceptionTagsLimitRule(1000, new List<string>()),
                new ScheduledNominationExceptionCodeLengthRule(1000),
                new ScheduledNominationExceptionManagerIdLengthRule(64),
            });
    }
}
