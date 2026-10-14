namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination schedule must pass. Check returns its problems, or none.</summary>
public interface INominationScheduleRule
{
    List<string> Check(NominationSchedule item);
}

public sealed record NominationScheduleNameLengthRule(int MaxLength) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationScheduleRequiredPhoneRule(string Message) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationScheduleRequiredCodeRule(string Message) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationScheduleRequiredNameRule(string Message) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationScheduleAllowedCodeRule(List<string> Allowed) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
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

public sealed record NominationScheduleCodePrefixRule(string Prefix) : INominationScheduleRule
{
    public List<string> Check(NominationSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination schedule.</summary>
public sealed record NominationScheduleValidator(List<INominationScheduleRule> Rules)
{
    public List<string> Validate(NominationSchedule item)
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

    public void RequireValid(NominationSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationSchedule> items)
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

/// <summary>Functions of nomination schedule rules.</summary>
public static class NominationScheduleRules
{
    public static NominationScheduleValidator DefaultNominationScheduleValidator()
    {
        return new NominationScheduleValidator(
            new List<INominationScheduleRule>
            {
                new NominationScheduleNameLengthRule(3600),
                new NominationScheduleRequiredPhoneRule("nomination schedule needs this field"),
                new NominationScheduleRequiredCodeRule("nomination schedule needs this field"),
                new NominationScheduleRequiredNameRule("nomination schedule needs an entry"),
                new NominationScheduleAllowedCodeRule(new List<string> { "fallback", "legacy" }),
                new NominationScheduleCodePrefixRule("ACM"),
            });
    }
}
