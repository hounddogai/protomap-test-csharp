namespace Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>A rule a visitor allocation must pass. Check returns its problems, or none.</summary>
public interface IVisitorAllocationRule
{
    List<string> Check(VisitorAllocation item);
}

public sealed record VisitorAllocationChannelLengthRule(int MaxLength) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAllocationCodePrefixRule(string Prefix) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAllocationFloorValueRangeRule(double Min, double Max) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorAllocationGraceDaysRangeRule(int Min, int Max) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAllocationLabelLengthRule(int MaxLength) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAllocationRequiredEmployeeIdRule(string Message) : IVisitorAllocationRule
{
    public List<string> Check(VisitorAllocation item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor allocation.</summary>
public sealed record VisitorAllocationValidator(List<IVisitorAllocationRule> Rules)
{
    public List<string> Validate(VisitorAllocation item)
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

    public void RequireValid(VisitorAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorAllocation> items)
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

/// <summary>Functions of visitor allocation rules.</summary>
public static class VisitorAllocationRules
{
    public static VisitorAllocationValidator DefaultVisitorAllocationValidator()
    {
        return new VisitorAllocationValidator(
            new List<IVisitorAllocationRule>
            {
                new VisitorAllocationChannelLengthRule(1000),
                new VisitorAllocationCodePrefixRule("X"),
                new VisitorAllocationFloorValueRangeRule(3.1, 2706.7),
                new VisitorAllocationGraceDaysRangeRule(80, 80),
                new VisitorAllocationLabelLengthRule(64),
                new VisitorAllocationRequiredEmployeeIdRule("visitor allocation needs a value"),
            });
    }
}
