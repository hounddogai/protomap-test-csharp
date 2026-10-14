namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled benchmark entry must pass. Check returns its problems, or none.</summary>
public interface IScheduledBenchmarkEntryRule
{
    List<string> Check(ScheduledBenchmarkEntry item);
}

public sealed record ScheduledBenchmarkEntryCodePrefixRule(string Prefix) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkEntryAllowedReviewerRule(List<string> Allowed) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkEntryRequiredPhoneRule(string Message) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkEntryPhoneLengthRule(int MaxLength) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkEntryCodeLengthRule(int MaxLength) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkEntryRequiredCodeRule(string Message) : IScheduledBenchmarkEntryRule
{
    public List<string> Check(ScheduledBenchmarkEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled benchmark entry.</summary>
public sealed record ScheduledBenchmarkEntryValidator(List<IScheduledBenchmarkEntryRule> Rules)
{
    public List<string> Validate(ScheduledBenchmarkEntry item)
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

    public void RequireValid(ScheduledBenchmarkEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBenchmarkEntry> items)
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

/// <summary>Functions of scheduled benchmark entry rules.</summary>
public static class ScheduledBenchmarkEntryRules
{
    public static ScheduledBenchmarkEntryValidator DefaultScheduledBenchmarkEntryValidator()
    {
        return new ScheduledBenchmarkEntryValidator(
            new List<IScheduledBenchmarkEntryRule>
            {
                new ScheduledBenchmarkEntryCodePrefixRule("REF"),
                new ScheduledBenchmarkEntryAllowedReviewerRule(new List<string> { "seasonal" }),
                new ScheduledBenchmarkEntryRequiredPhoneRule("scheduled benchmark entry needs an entry"),
                new ScheduledBenchmarkEntryPhoneLengthRule(255),
                new ScheduledBenchmarkEntryCodeLengthRule(3600),
                new ScheduledBenchmarkEntryRequiredCodeRule("scheduled benchmark entry needs this field"),
            });
    }
}
