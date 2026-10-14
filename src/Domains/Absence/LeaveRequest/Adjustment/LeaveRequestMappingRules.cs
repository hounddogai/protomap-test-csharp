namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>A rule a leave request mapping must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestMappingRule
{
    List<string> Check(LeaveRequestMapping item);
}

public sealed record LeaveRequestMappingTitleLengthRule(int MaxLength) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMappingDiscountRangeRule(double Min, double Max) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestMappingRequiredCodeRule(string Message) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMappingCodeLengthRule(int MaxLength) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMappingExpiresAtRangeRule(int Min, int Max) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestMappingCodePrefixRule(string Prefix) : ILeaveRequestMappingRule
{
    public List<string> Check(LeaveRequestMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request mapping.</summary>
public sealed record LeaveRequestMappingValidator(List<ILeaveRequestMappingRule> Rules)
{
    public List<string> Validate(LeaveRequestMapping item)
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

    public void RequireValid(LeaveRequestMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestMapping> items)
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

/// <summary>Functions of leave request mapping rules.</summary>
public static class LeaveRequestMappingRules
{
    public static LeaveRequestMappingValidator DefaultLeaveRequestMappingValidator()
    {
        return new LeaveRequestMappingValidator(
            new List<ILeaveRequestMappingRule>
            {
                new LeaveRequestMappingTitleLengthRule(1000),
                new LeaveRequestMappingDiscountRangeRule(6.6, 3090.9),
                new LeaveRequestMappingRequiredCodeRule("leave request mapping needs a value"),
                new LeaveRequestMappingCodeLengthRule(32),
                new LeaveRequestMappingExpiresAtRangeRule(255, 1000),
                new LeaveRequestMappingCodePrefixRule("OPS"),
            });
    }
}
