namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall notice must pass. Check returns its problems, or none.</summary>
public interface ITownHallNoticeRule
{
    List<string> Check(TownHallNotice item);
}

public sealed record TownHallNoticeNameLengthRule(int MaxLength) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallNoticeRequiredNotesRule(string Message) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallNoticeCodePrefixRule(string Prefix) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallNoticeAllowedNameRule(List<string> Allowed) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record TownHallNoticeNotesLengthRule(int MaxLength) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallNoticeRequiredCodeRule(string Message) : ITownHallNoticeRule
{
    public List<string> Check(TownHallNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall notice.</summary>
public sealed record TownHallNoticeValidator(List<ITownHallNoticeRule> Rules)
{
    public List<string> Validate(TownHallNotice item)
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

    public void RequireValid(TownHallNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallNotice> items)
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

/// <summary>Functions of town hall notice rules.</summary>
public static class TownHallNoticeRules
{
    public static TownHallNoticeValidator DefaultTownHallNoticeValidator()
    {
        return new TownHallNoticeValidator(
            new List<ITownHallNoticeRule>
            {
                new TownHallNoticeNameLengthRule(3600),
                new TownHallNoticeRequiredNotesRule("town hall notice needs a value"),
                new TownHallNoticeCodePrefixRule("ACM"),
                new TownHallNoticeAllowedNameRule(new List<string> { "fallback", "batched" }),
                new TownHallNoticeNotesLengthRule(86400),
                new TownHallNoticeRequiredCodeRule("town hall notice needs an entry"),
            });
    }
}
