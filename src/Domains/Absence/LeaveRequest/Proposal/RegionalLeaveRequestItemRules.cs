namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>A rule a regional leave request item must pass. Check returns its problems, or none.</summary>
public interface IRegionalLeaveRequestItemRule
{
    List<string> Check(RegionalLeaveRequestItem item);
}

public sealed record RegionalLeaveRequestItemRequiredLocationRule(string Message) : IRegionalLeaveRequestItemRule
{
    public List<string> Check(RegionalLeaveRequestItem item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestItemRequiredCodeRule(string Message) : IRegionalLeaveRequestItemRule
{
    public List<string> Check(RegionalLeaveRequestItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLeaveRequestItemSummaryLengthRule(int MaxLength) : IRegionalLeaveRequestItemRule
{
    public List<string> Check(RegionalLeaveRequestItem item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional leave request item.</summary>
public sealed record RegionalLeaveRequestItemValidator(List<IRegionalLeaveRequestItemRule> Rules)
{
    public List<string> Validate(RegionalLeaveRequestItem item)
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

    public void RequireValid(RegionalLeaveRequestItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalLeaveRequestItem> items)
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

/// <summary>Functions of regional leave request item rules.</summary>
public static class RegionalLeaveRequestItemRules
{
    public static RegionalLeaveRequestItemValidator DefaultRegionalLeaveRequestItemValidator()
    {
        return new RegionalLeaveRequestItemValidator(
            new List<IRegionalLeaveRequestItemRule>
            {
                new RegionalLeaveRequestItemRequiredLocationRule("regional leave request item needs this field"),
                new RegionalLeaveRequestItemRequiredCodeRule("regional leave request item needs a value"),
                new RegionalLeaveRequestItemSummaryLengthRule(255),
            });
    }
}
