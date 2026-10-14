namespace Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>A rule a leave request claim must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestClaimRule
{
    List<string> Check(LeaveRequestClaim item);
}

public sealed record LeaveRequestClaimRequiredTitleRule(string Message) : ILeaveRequestClaimRule
{
    public List<string> Check(LeaveRequestClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestClaimRequiredCurrencyRule(string Message) : ILeaveRequestClaimRule
{
    public List<string> Check(LeaveRequestClaim item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestClaimTagsLimitRule(int MaxCount, List<string> Banned) : ILeaveRequestClaimRule
{
    public List<string> Check(LeaveRequestClaim item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a leave request claim.</summary>
public sealed record LeaveRequestClaimValidator(List<ILeaveRequestClaimRule> Rules)
{
    public List<string> Validate(LeaveRequestClaim item)
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

    public void RequireValid(LeaveRequestClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestClaim> items)
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

/// <summary>Functions of leave request claim rules.</summary>
public static class LeaveRequestClaimRules
{
    public static LeaveRequestClaimValidator DefaultLeaveRequestClaimValidator()
    {
        return new LeaveRequestClaimValidator(
            new List<ILeaveRequestClaimRule>
            {
                new LeaveRequestClaimRequiredTitleRule("leave request claim needs an entry"),
                new LeaveRequestClaimRequiredCurrencyRule("leave request claim needs this field"),
                new LeaveRequestClaimTagsLimitRule(16, new List<string> { "fallback", "capped", "rolling" }),
            });
    }
}
