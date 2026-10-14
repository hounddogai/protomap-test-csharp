namespace Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>A rule a capped leave request baseline must pass. Check returns its problems, or none.</summary>
public interface ICappedLeaveRequestBaselineRule
{
    List<string> Check(CappedLeaveRequestBaseline item);
}

public sealed record CappedLeaveRequestBaselineRequiredCodeRule(string Message) : ICappedLeaveRequestBaselineRule
{
    public List<string> Check(CappedLeaveRequestBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLeaveRequestBaselineCapacityRangeRule(int Min, int Max) : ICappedLeaveRequestBaselineRule
{
    public List<string> Check(CappedLeaveRequestBaseline item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedLeaveRequestBaselineCountryLengthRule(int MaxLength) : ICappedLeaveRequestBaselineRule
{
    public List<string> Check(CappedLeaveRequestBaseline item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped leave request baseline.</summary>
public sealed record CappedLeaveRequestBaselineValidator(List<ICappedLeaveRequestBaselineRule> Rules)
{
    public List<string> Validate(CappedLeaveRequestBaseline item)
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

    public void RequireValid(CappedLeaveRequestBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLeaveRequestBaseline> items)
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

/// <summary>Functions of capped leave request baseline rules.</summary>
public static class CappedLeaveRequestBaselineRules
{
    public static CappedLeaveRequestBaselineValidator DefaultCappedLeaveRequestBaselineValidator()
    {
        return new CappedLeaveRequestBaselineValidator(
            new List<ICappedLeaveRequestBaselineRule>
            {
                new CappedLeaveRequestBaselineRequiredCodeRule("capped leave request baseline needs an entry"),
                new CappedLeaveRequestBaselineCapacityRangeRule(32, 80),
                new CappedLeaveRequestBaselineCountryLengthRule(64),
            });
    }
}
