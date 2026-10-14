namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary merit cycle allocation must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMeritCycleAllocationRule
{
    List<string> Check(PrimaryMeritCycleAllocation item);
}

public sealed record PrimaryMeritCycleAllocationAllowedNotesRule(List<string> Allowed) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleAllocationTitleLengthRule(int MaxLength) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleAllocationRequiredTitleRule(string Message) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleAllocationCodePrefixRule(string Prefix) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleAllocationRequiredCodeRule(string Message) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleAllocationLeaveDaysRangeRule(int Min, int Max) : IPrimaryMeritCycleAllocationRule
{
    public List<string> Check(PrimaryMeritCycleAllocation item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary merit cycle allocation.</summary>
public sealed record PrimaryMeritCycleAllocationValidator(List<IPrimaryMeritCycleAllocationRule> Rules)
{
    public List<string> Validate(PrimaryMeritCycleAllocation item)
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

    public void RequireValid(PrimaryMeritCycleAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMeritCycleAllocation> items)
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

/// <summary>Functions of primary merit cycle allocation rules.</summary>
public static class PrimaryMeritCycleAllocationRules
{
    public static PrimaryMeritCycleAllocationValidator DefaultPrimaryMeritCycleAllocationValidator()
    {
        return new PrimaryMeritCycleAllocationValidator(
            new List<IPrimaryMeritCycleAllocationRule>
            {
                new PrimaryMeritCycleAllocationAllowedNotesRule(new List<string>()),
                new PrimaryMeritCycleAllocationTitleLengthRule(1000),
                new PrimaryMeritCycleAllocationRequiredTitleRule("primary merit cycle allocation needs this field"),
                new PrimaryMeritCycleAllocationCodePrefixRule("REF"),
                new PrimaryMeritCycleAllocationRequiredCodeRule("primary merit cycle allocation needs a value"),
                new PrimaryMeritCycleAllocationLeaveDaysRangeRule(3600, 86400),
            });
    }
}
