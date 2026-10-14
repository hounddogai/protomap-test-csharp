namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg schedule must pass. Check returns its problems, or none.</summary>
public interface IReorgScheduleRule
{
    List<string> Check(ReorgSchedule item);
}

public sealed record ReorgScheduleLabelLengthRule(int MaxLength) : IReorgScheduleRule
{
    public List<string> Check(ReorgSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgScheduleCodePrefixRule(string Prefix) : IReorgScheduleRule
{
    public List<string> Check(ReorgSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgScheduleRequiredPhoneRule(string Message) : IReorgScheduleRule
{
    public List<string> Check(ReorgSchedule item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgScheduleAllowedLocationRule(List<string> Allowed) : IReorgScheduleRule
{
    public List<string> Check(ReorgSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ReorgScheduleRequiredCodeRule(string Message) : IReorgScheduleRule
{
    public List<string> Check(ReorgSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg schedule.</summary>
public sealed record ReorgScheduleValidator(List<IReorgScheduleRule> Rules)
{
    public List<string> Validate(ReorgSchedule item)
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

    public void RequireValid(ReorgSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgSchedule> items)
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

/// <summary>Functions of reorg schedule rules.</summary>
public static class ReorgScheduleRules
{
    public static ReorgScheduleValidator DefaultReorgScheduleValidator()
    {
        return new ReorgScheduleValidator(
            new List<IReorgScheduleRule>
            {
                new ReorgScheduleLabelLengthRule(32),
                new ReorgScheduleCodePrefixRule("ACM"),
                new ReorgScheduleRequiredPhoneRule("reorg schedule needs this field"),
                new ReorgScheduleAllowedLocationRule(new List<string> { "scheduled", "default" }),
                new ReorgScheduleRequiredCodeRule("reorg schedule needs an entry"),
            });
    }
}
