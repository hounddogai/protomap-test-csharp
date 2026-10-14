namespace Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>A rule a tiered leave request item must pass. Check returns its problems, or none.</summary>
public interface ITieredLeaveRequestItemRule
{
    List<string> Check(TieredLeaveRequestItem item);
}

public sealed record TieredLeaveRequestItemStartsAtRangeRule(int Min, int Max) : ITieredLeaveRequestItemRule
{
    public List<string> Check(TieredLeaveRequestItem item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestItemRequiredLocaleRule(string Message) : ITieredLeaveRequestItemRule
{
    public List<string> Check(TieredLeaveRequestItem item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestItemRequiredTitleRule(string Message) : ITieredLeaveRequestItemRule
{
    public List<string> Check(TieredLeaveRequestItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestItemTeamLengthRule(int MaxLength) : ITieredLeaveRequestItemRule
{
    public List<string> Check(TieredLeaveRequestItem item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered leave request item.</summary>
public sealed record TieredLeaveRequestItemValidator(List<ITieredLeaveRequestItemRule> Rules)
{
    public List<string> Validate(TieredLeaveRequestItem item)
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

    public void RequireValid(TieredLeaveRequestItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLeaveRequestItem> items)
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

/// <summary>Functions of tiered leave request item rules.</summary>
public static class TieredLeaveRequestItemRules
{
    public static TieredLeaveRequestItemValidator DefaultTieredLeaveRequestItemValidator()
    {
        return new TieredLeaveRequestItemValidator(
            new List<ITieredLeaveRequestItemRule>
            {
                new TieredLeaveRequestItemStartsAtRangeRule(80, 80),
                new TieredLeaveRequestItemRequiredLocaleRule("tiered leave request item needs this field"),
                new TieredLeaveRequestItemRequiredTitleRule("tiered leave request item needs this field"),
                new TieredLeaveRequestItemTeamLengthRule(255),
            });
    }
}
