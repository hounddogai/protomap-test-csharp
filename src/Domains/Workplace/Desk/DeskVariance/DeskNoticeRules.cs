namespace Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>A rule a desk notice must pass. Check returns its problems, or none.</summary>
public interface IDeskNoticeRule
{
    List<string> Check(DeskNotice item);
}

public sealed record DeskNoticeRequiredTitleRule(string Message) : IDeskNoticeRule
{
    public List<string> Check(DeskNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskNoticeTitleLengthRule(int MaxLength) : IDeskNoticeRule
{
    public List<string> Check(DeskNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskNoticeCodePrefixRule(string Prefix) : IDeskNoticeRule
{
    public List<string> Check(DeskNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskNoticeRequiredTimezoneRule(string Message) : IDeskNoticeRule
{
    public List<string> Check(DeskNotice item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk notice.</summary>
public sealed record DeskNoticeValidator(List<IDeskNoticeRule> Rules)
{
    public List<string> Validate(DeskNotice item)
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

    public void RequireValid(DeskNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskNotice> items)
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

/// <summary>Functions of desk notice rules.</summary>
public static class DeskNoticeRules
{
    public static DeskNoticeValidator DefaultDeskNoticeValidator()
    {
        return new DeskNoticeValidator(
            new List<IDeskNoticeRule>
            {
                new DeskNoticeRequiredTitleRule("desk notice needs a value"),
                new DeskNoticeTitleLengthRule(16),
                new DeskNoticeCodePrefixRule("OPS"),
                new DeskNoticeRequiredTimezoneRule("desk notice needs a value"),
            });
    }
}
