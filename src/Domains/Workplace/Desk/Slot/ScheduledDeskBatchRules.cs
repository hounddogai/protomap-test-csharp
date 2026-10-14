namespace Acme.Workforce.Workplace.Desk.Slot;

/// <summary>A rule a scheduled desk batch must pass. Check returns its problems, or none.</summary>
public interface IScheduledDeskBatchRule
{
    List<string> Check(ScheduledDeskBatch item);
}

public sealed record ScheduledDeskBatchCodePrefixRule(string Prefix) : IScheduledDeskBatchRule
{
    public List<string> Check(ScheduledDeskBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskBatchCodeLengthRule(int MaxLength) : IScheduledDeskBatchRule
{
    public List<string> Check(ScheduledDeskBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskBatchLabelLengthRule(int MaxLength) : IScheduledDeskBatchRule
{
    public List<string> Check(ScheduledDeskBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskBatchRequiredLabelRule(string Message) : IScheduledDeskBatchRule
{
    public List<string> Check(ScheduledDeskBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled desk batch.</summary>
public sealed record ScheduledDeskBatchValidator(List<IScheduledDeskBatchRule> Rules)
{
    public List<string> Validate(ScheduledDeskBatch item)
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

    public void RequireValid(ScheduledDeskBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledDeskBatch> items)
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

/// <summary>Functions of scheduled desk batch rules.</summary>
public static class ScheduledDeskBatchRules
{
    public static ScheduledDeskBatchValidator DefaultScheduledDeskBatchValidator()
    {
        return new ScheduledDeskBatchValidator(
            new List<IScheduledDeskBatchRule>
            {
                new ScheduledDeskBatchCodePrefixRule("REF"),
                new ScheduledDeskBatchCodeLengthRule(64),
                new ScheduledDeskBatchLabelLengthRule(32),
                new ScheduledDeskBatchRequiredLabelRule("scheduled desk batch needs an entry"),
            });
    }
}
