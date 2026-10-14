namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>A rule a regional visitor allocation must pass. Check returns its problems, or none.</summary>
public interface IRegionalVisitorAllocationRule
{
    List<string> Check(RegionalVisitorAllocation item);
}

public sealed record RegionalVisitorAllocationRequiredCodeRule(string Message) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorAllocationRetriesRangeRule(int Min, int Max) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorAllocationCodeLengthRule(int MaxLength) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorAllocationLeaveDaysRangeRule(int Min, int Max) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorAllocationRequiredLabelRule(string Message) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorAllocationCodePrefixRule(string Prefix) : IRegionalVisitorAllocationRule
{
    public List<string> Check(RegionalVisitorAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional visitor allocation.</summary>
public sealed record RegionalVisitorAllocationValidator(List<IRegionalVisitorAllocationRule> Rules)
{
    public List<string> Validate(RegionalVisitorAllocation item)
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

    public void RequireValid(RegionalVisitorAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalVisitorAllocation> items)
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

/// <summary>Functions of regional visitor allocation rules.</summary>
public static class RegionalVisitorAllocationRules
{
    public static RegionalVisitorAllocationValidator DefaultRegionalVisitorAllocationValidator()
    {
        return new RegionalVisitorAllocationValidator(
            new List<IRegionalVisitorAllocationRule>
            {
                new RegionalVisitorAllocationRequiredCodeRule("regional visitor allocation needs this field"),
                new RegionalVisitorAllocationRetriesRangeRule(80, 86400),
                new RegionalVisitorAllocationCodeLengthRule(64),
                new RegionalVisitorAllocationLeaveDaysRangeRule(16, 16),
                new RegionalVisitorAllocationRequiredLabelRule("regional visitor allocation needs a value"),
                new RegionalVisitorAllocationCodePrefixRule("Q"),
            });
    }
}
