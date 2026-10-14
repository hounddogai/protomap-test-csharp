namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>A rule a leave request profile must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestProfileRule
{
    List<string> Check(LeaveRequestProfile item);
}

public sealed record LeaveRequestProfileLocationLengthRule(int MaxLength) : ILeaveRequestProfileRule
{
    public List<string> Check(LeaveRequestProfile item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProfileCodePrefixRule(string Prefix) : ILeaveRequestProfileRule
{
    public List<string> Check(LeaveRequestProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProfileRequiredCodeRule(string Message) : ILeaveRequestProfileRule
{
    public List<string> Check(LeaveRequestProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProfileRequiredLabelRule(string Message) : ILeaveRequestProfileRule
{
    public List<string> Check(LeaveRequestProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProfileRateRangeRule(double Min, double Max) : ILeaveRequestProfileRule
{
    public List<string> Check(LeaveRequestProfile item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request profile.</summary>
public sealed record LeaveRequestProfileValidator(List<ILeaveRequestProfileRule> Rules)
{
    public List<string> Validate(LeaveRequestProfile item)
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

    public void RequireValid(LeaveRequestProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestProfile> items)
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

/// <summary>Functions of leave request profile rules.</summary>
public static class LeaveRequestProfileRules
{
    public static LeaveRequestProfileValidator DefaultLeaveRequestProfileValidator()
    {
        return new LeaveRequestProfileValidator(
            new List<ILeaveRequestProfileRule>
            {
                new LeaveRequestProfileLocationLengthRule(64),
                new LeaveRequestProfileCodePrefixRule("REF"),
                new LeaveRequestProfileRequiredCodeRule("leave request profile needs this field"),
                new LeaveRequestProfileRequiredLabelRule("leave request profile needs this field"),
                new LeaveRequestProfileRateRangeRule(2.1, 1083.3),
            });
    }
}
