namespace Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>A rule a pending access badge request must pass. Check returns its problems, or none.</summary>
public interface IPendingAccessBadgeRequestRule
{
    List<string> Check(PendingAccessBadgeRequest item);
}

public sealed record PendingAccessBadgeRequestRevisionRangeRule(int Min, int Max) : IPendingAccessBadgeRequestRule
{
    public List<string> Check(PendingAccessBadgeRequest item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeRequestRequiredCodeRule(string Message) : IPendingAccessBadgeRequestRule
{
    public List<string> Check(PendingAccessBadgeRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAccessBadgeRequestCodeLengthRule(int MaxLength) : IPendingAccessBadgeRequestRule
{
    public List<string> Check(PendingAccessBadgeRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending access badge request.</summary>
public sealed record PendingAccessBadgeRequestValidator(List<IPendingAccessBadgeRequestRule> Rules)
{
    public List<string> Validate(PendingAccessBadgeRequest item)
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

    public void RequireValid(PendingAccessBadgeRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAccessBadgeRequest> items)
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

/// <summary>Functions of pending access badge request rules.</summary>
public static class PendingAccessBadgeRequestRules
{
    public static PendingAccessBadgeRequestValidator DefaultPendingAccessBadgeRequestValidator()
    {
        return new PendingAccessBadgeRequestValidator(
            new List<IPendingAccessBadgeRequestRule>
            {
                new PendingAccessBadgeRequestRevisionRangeRule(16, 16),
                new PendingAccessBadgeRequestRequiredCodeRule("pending access badge request needs a value"),
                new PendingAccessBadgeRequestCodeLengthRule(16),
            });
    }
}
