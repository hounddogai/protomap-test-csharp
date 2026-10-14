namespace Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>A rule a leave request rule must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestRuleRule
{
    List<string> Check(LeaveRequestRule item);
}

public sealed record LeaveRequestRuleRequiredCountryRule(string Message) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRuleDescriptionLengthRule(int MaxLength) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRuleLabelLengthRule(int MaxLength) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRuleCodePrefixRule(string Prefix) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRuleMaximumRangeRule(double Min, double Max) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestRuleCapacityRangeRule(int Min, int Max) : ILeaveRequestRuleRule
{
    public List<string> Check(LeaveRequestRule item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request rule.</summary>
public sealed record LeaveRequestRuleValidator(List<ILeaveRequestRuleRule> Rules)
{
    public List<string> Validate(LeaveRequestRule item)
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

    public void RequireValid(LeaveRequestRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestRule> items)
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

/// <summary>Functions of leave request rule rules.</summary>
public static class LeaveRequestRuleRules
{
    public static LeaveRequestRuleValidator DefaultLeaveRequestRuleValidator()
    {
        return new LeaveRequestRuleValidator(
            new List<ILeaveRequestRuleRule>
            {
                new LeaveRequestRuleRequiredCountryRule("leave request rule needs this field"),
                new LeaveRequestRuleDescriptionLengthRule(16),
                new LeaveRequestRuleLabelLengthRule(255),
                new LeaveRequestRuleCodePrefixRule("REF"),
                new LeaveRequestRuleMaximumRangeRule(6.7, 2796.2),
                new LeaveRequestRuleCapacityRangeRule(3600, 3600),
            });
    }
}
