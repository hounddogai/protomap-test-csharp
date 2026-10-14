namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request baseline must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestBaselineRule
{
    List<string> Check(LeaveRequestBaseline item);
}

public sealed record LeaveRequestBaselineAllowedTitleRule(List<string> Allowed) : ILeaveRequestBaselineRule
{
    public List<string> Check(LeaveRequestBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBaselineCurrencyLengthRule(int MaxLength) : ILeaveRequestBaselineRule
{
    public List<string> Check(LeaveRequestBaseline item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBaselineRequiredCurrencyRule(string Message) : ILeaveRequestBaselineRule
{
    public List<string> Check(LeaveRequestBaseline item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request baseline.</summary>
public sealed record LeaveRequestBaselineValidator(List<ILeaveRequestBaselineRule> Rules)
{
    public List<string> Validate(LeaveRequestBaseline item)
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

    public void RequireValid(LeaveRequestBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestBaseline> items)
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

/// <summary>Functions of leave request baseline rules.</summary>
public static class LeaveRequestBaselineRules
{
    public static LeaveRequestBaselineValidator DefaultLeaveRequestBaselineValidator()
    {
        return new LeaveRequestBaselineValidator(
            new List<ILeaveRequestBaselineRule>
            {
                new LeaveRequestBaselineAllowedTitleRule(new List<string> { "tiered" }),
                new LeaveRequestBaselineCurrencyLengthRule(64),
                new LeaveRequestBaselineRequiredCurrencyRule("leave request baseline needs an entry"),
            });
    }
}
