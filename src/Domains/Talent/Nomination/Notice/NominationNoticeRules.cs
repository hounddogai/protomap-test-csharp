namespace Acme.Workforce.Talent.Nomination.Notice;

/// <summary>A rule a nomination notice must pass. Check returns its problems, or none.</summary>
public interface INominationNoticeRule
{
    List<string> Check(NominationNotice item);
}

public sealed record NominationNoticeTitleLengthRule(int MaxLength) : INominationNoticeRule
{
    public List<string> Check(NominationNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationNoticeOwnerIdLengthRule(int MaxLength) : INominationNoticeRule
{
    public List<string> Check(NominationNotice item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationNoticeLocationLengthRule(int MaxLength) : INominationNoticeRule
{
    public List<string> Check(NominationNotice item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationNoticeAttemptsRangeRule(int Min, int Max) : INominationNoticeRule
{
    public List<string> Check(NominationNotice item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationNoticeRequiredOwnerIdRule(string Message) : INominationNoticeRule
{
    public List<string> Check(NominationNotice item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination notice.</summary>
public sealed record NominationNoticeValidator(List<INominationNoticeRule> Rules)
{
    public List<string> Validate(NominationNotice item)
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

    public void RequireValid(NominationNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationNotice> items)
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

/// <summary>Functions of nomination notice rules.</summary>
public static class NominationNoticeRules
{
    public static NominationNoticeValidator DefaultNominationNoticeValidator()
    {
        return new NominationNoticeValidator(
            new List<INominationNoticeRule>
            {
                new NominationNoticeTitleLengthRule(80),
                new NominationNoticeOwnerIdLengthRule(16),
                new NominationNoticeLocationLengthRule(3600),
                new NominationNoticeAttemptsRangeRule(32, 120),
                new NominationNoticeRequiredOwnerIdRule("nomination notice needs this field"),
            });
    }
}
