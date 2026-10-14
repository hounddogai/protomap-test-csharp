namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request bundle must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestBundleRule
{
    List<string> Check(LeaveRequestBundle item);
}

public sealed record LeaveRequestBundleCodeLengthRule(int MaxLength) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBundleWindowDaysRangeRule(int Min, int Max) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBundleCodePrefixRule(string Prefix) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBundleAllowedNameRule(List<string> Allowed) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBundleRequiredSourceRule(string Message) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBundleRequiredCodeRule(string Message) : ILeaveRequestBundleRule
{
    public List<string> Check(LeaveRequestBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request bundle.</summary>
public sealed record LeaveRequestBundleValidator(List<ILeaveRequestBundleRule> Rules)
{
    public List<string> Validate(LeaveRequestBundle item)
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

    public void RequireValid(LeaveRequestBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestBundle> items)
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

/// <summary>Functions of leave request bundle rules.</summary>
public static class LeaveRequestBundleRules
{
    public static LeaveRequestBundleValidator DefaultLeaveRequestBundleValidator()
    {
        return new LeaveRequestBundleValidator(
            new List<ILeaveRequestBundleRule>
            {
                new LeaveRequestBundleCodeLengthRule(1000),
                new LeaveRequestBundleWindowDaysRangeRule(16, 255),
                new LeaveRequestBundleCodePrefixRule("OPS"),
                new LeaveRequestBundleAllowedNameRule(new List<string> { "batched" }),
                new LeaveRequestBundleRequiredSourceRule("leave request bundle needs a value"),
                new LeaveRequestBundleRequiredCodeRule("leave request bundle needs a value"),
            });
    }
}
