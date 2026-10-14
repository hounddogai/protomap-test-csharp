namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>A rule a primary garnishment allocation must pass. Check returns its problems, or none.</summary>
public interface IPrimaryGarnishmentAllocationRule
{
    List<string> Check(PrimaryGarnishmentAllocation item);
}

public sealed record PrimaryGarnishmentAllocationReviewerLengthRule(int MaxLength) : IPrimaryGarnishmentAllocationRule
{
    public List<string> Check(PrimaryGarnishmentAllocation item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentAllocationChannelLengthRule(int MaxLength) : IPrimaryGarnishmentAllocationRule
{
    public List<string> Check(PrimaryGarnishmentAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentAllocationCodeLengthRule(int MaxLength) : IPrimaryGarnishmentAllocationRule
{
    public List<string> Check(PrimaryGarnishmentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentAllocationDurationMinutesRangeRule(int Min, int Max) : IPrimaryGarnishmentAllocationRule
{
    public List<string> Check(PrimaryGarnishmentAllocation item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentAllocationMaximumRangeRule(double Min, double Max) : IPrimaryGarnishmentAllocationRule
{
    public List<string> Check(PrimaryGarnishmentAllocation item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary garnishment allocation.</summary>
public sealed record PrimaryGarnishmentAllocationValidator(List<IPrimaryGarnishmentAllocationRule> Rules)
{
    public List<string> Validate(PrimaryGarnishmentAllocation item)
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

    public void RequireValid(PrimaryGarnishmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryGarnishmentAllocation> items)
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

/// <summary>Functions of primary garnishment allocation rules.</summary>
public static class PrimaryGarnishmentAllocationRules
{
    public static PrimaryGarnishmentAllocationValidator DefaultPrimaryGarnishmentAllocationValidator()
    {
        return new PrimaryGarnishmentAllocationValidator(
            new List<IPrimaryGarnishmentAllocationRule>
            {
                new PrimaryGarnishmentAllocationReviewerLengthRule(64),
                new PrimaryGarnishmentAllocationChannelLengthRule(64),
                new PrimaryGarnishmentAllocationCodeLengthRule(1000),
                new PrimaryGarnishmentAllocationDurationMinutesRangeRule(32, 80),
                new PrimaryGarnishmentAllocationMaximumRangeRule(1.7, 750.5),
            });
    }
}
