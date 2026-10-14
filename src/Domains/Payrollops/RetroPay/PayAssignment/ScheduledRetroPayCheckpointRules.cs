namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled retro pay checkpoint must pass. Check returns its problems, or none.</summary>
public interface IScheduledRetroPayCheckpointRule
{
    List<string> Check(ScheduledRetroPayCheckpoint item);
}

public sealed record ScheduledRetroPayCheckpointCodeLengthRule(int MaxLength) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayCheckpointRequiredLocaleRule(string Message) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayCheckpointRequiredCodeRule(string Message) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayCheckpointAllowedOwnerIdRule(List<string> Allowed) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayCheckpointCodePrefixRule(string Prefix) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayCheckpointRequiredLabelRule(string Message) : IScheduledRetroPayCheckpointRule
{
    public List<string> Check(ScheduledRetroPayCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled retro pay checkpoint.</summary>
public sealed record ScheduledRetroPayCheckpointValidator(List<IScheduledRetroPayCheckpointRule> Rules)
{
    public List<string> Validate(ScheduledRetroPayCheckpoint item)
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

    public void RequireValid(ScheduledRetroPayCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRetroPayCheckpoint> items)
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

/// <summary>Functions of scheduled retro pay checkpoint rules.</summary>
public static class ScheduledRetroPayCheckpointRules
{
    public static ScheduledRetroPayCheckpointValidator DefaultScheduledRetroPayCheckpointValidator()
    {
        return new ScheduledRetroPayCheckpointValidator(
            new List<IScheduledRetroPayCheckpointRule>
            {
                new ScheduledRetroPayCheckpointCodeLengthRule(1000),
                new ScheduledRetroPayCheckpointRequiredLocaleRule("scheduled retro pay checkpoint needs this field"),
                new ScheduledRetroPayCheckpointRequiredCodeRule("scheduled retro pay checkpoint needs this field"),
                new ScheduledRetroPayCheckpointAllowedOwnerIdRule(new List<string> { "tiered" }),
                new ScheduledRetroPayCheckpointCodePrefixRule("REF"),
                new ScheduledRetroPayCheckpointRequiredLabelRule("scheduled retro pay checkpoint needs an entry"),
            });
    }
}
