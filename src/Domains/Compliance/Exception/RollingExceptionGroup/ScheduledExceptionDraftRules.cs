namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>A rule a scheduled exception draft must pass. Check returns its problems, or none.</summary>
public interface IScheduledExceptionDraftRule
{
    List<string> Check(ScheduledExceptionDraft item);
}

public sealed record ScheduledExceptionDraftRequiredCodeRule(string Message) : IScheduledExceptionDraftRule
{
    public List<string> Check(ScheduledExceptionDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionDraftCodeLengthRule(int MaxLength) : IScheduledExceptionDraftRule
{
    public List<string> Check(ScheduledExceptionDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionDraftRequiredLabelRule(string Message) : IScheduledExceptionDraftRule
{
    public List<string> Check(ScheduledExceptionDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionDraftCodePrefixRule(string Prefix) : IScheduledExceptionDraftRule
{
    public List<string> Check(ScheduledExceptionDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledExceptionDraftLabelLengthRule(int MaxLength) : IScheduledExceptionDraftRule
{
    public List<string> Check(ScheduledExceptionDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled exception draft.</summary>
public sealed record ScheduledExceptionDraftValidator(List<IScheduledExceptionDraftRule> Rules)
{
    public List<string> Validate(ScheduledExceptionDraft item)
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

    public void RequireValid(ScheduledExceptionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledExceptionDraft> items)
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

/// <summary>Functions of scheduled exception draft rules.</summary>
public static class ScheduledExceptionDraftRules
{
    public static ScheduledExceptionDraftValidator DefaultScheduledExceptionDraftValidator()
    {
        return new ScheduledExceptionDraftValidator(
            new List<IScheduledExceptionDraftRule>
            {
                new ScheduledExceptionDraftRequiredCodeRule("scheduled exception draft needs a value"),
                new ScheduledExceptionDraftCodeLengthRule(16),
                new ScheduledExceptionDraftRequiredLabelRule("scheduled exception draft needs an entry"),
                new ScheduledExceptionDraftCodePrefixRule("Q"),
                new ScheduledExceptionDraftLabelLengthRule(255),
            });
    }
}
