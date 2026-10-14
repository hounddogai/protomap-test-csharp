namespace Acme.Workforce.Talent.Mobility.Tier;

/// <summary>A rule a scheduled mobility request must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityRequestRule
{
    List<string> Check(ScheduledMobilityRequest item);
}

public sealed record ScheduledMobilityRequestRequiredNameRule(string Message) : IScheduledMobilityRequestRule
{
    public List<string> Check(ScheduledMobilityRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRequestAmountRangeRule(double Min, double Max) : IScheduledMobilityRequestRule
{
    public List<string> Check(ScheduledMobilityRequest item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledMobilityRequestEmployeeIdLengthRule(int MaxLength) : IScheduledMobilityRequestRule
{
    public List<string> Check(ScheduledMobilityRequest item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRequestNameLengthRule(int MaxLength) : IScheduledMobilityRequestRule
{
    public List<string> Check(ScheduledMobilityRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRequestCodeLengthRule(int MaxLength) : IScheduledMobilityRequestRule
{
    public List<string> Check(ScheduledMobilityRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility request.</summary>
public sealed record ScheduledMobilityRequestValidator(List<IScheduledMobilityRequestRule> Rules)
{
    public List<string> Validate(ScheduledMobilityRequest item)
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

    public void RequireValid(ScheduledMobilityRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityRequest> items)
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

/// <summary>Functions of scheduled mobility request rules.</summary>
public static class ScheduledMobilityRequestRules
{
    public static ScheduledMobilityRequestValidator DefaultScheduledMobilityRequestValidator()
    {
        return new ScheduledMobilityRequestValidator(
            new List<IScheduledMobilityRequestRule>
            {
                new ScheduledMobilityRequestRequiredNameRule("scheduled mobility request needs a value"),
                new ScheduledMobilityRequestAmountRangeRule(7.5, 3877.6),
                new ScheduledMobilityRequestEmployeeIdLengthRule(3600),
                new ScheduledMobilityRequestNameLengthRule(86400),
                new ScheduledMobilityRequestCodeLengthRule(255),
            });
    }
}
