namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>A rule a license seat allocation must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatAllocationRule
{
    List<string> Check(LicenseSeatAllocation item);
}

public sealed record LicenseSeatAllocationRequiredSourceRule(string Message) : ILicenseSeatAllocationRule
{
    public List<string> Check(LicenseSeatAllocation item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAllocationCodePrefixRule(string Prefix) : ILicenseSeatAllocationRule
{
    public List<string> Check(LicenseSeatAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAllocationCapacityRangeRule(int Min, int Max) : ILicenseSeatAllocationRule
{
    public List<string> Check(LicenseSeatAllocation item)
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

public sealed record LicenseSeatAllocationRequiredCodeRule(string Message) : ILicenseSeatAllocationRule
{
    public List<string> Check(LicenseSeatAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat allocation.</summary>
public sealed record LicenseSeatAllocationValidator(List<ILicenseSeatAllocationRule> Rules)
{
    public List<string> Validate(LicenseSeatAllocation item)
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

    public void RequireValid(LicenseSeatAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatAllocation> items)
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

/// <summary>Functions of license seat allocation rules.</summary>
public static class LicenseSeatAllocationRules
{
    public static LicenseSeatAllocationValidator DefaultLicenseSeatAllocationValidator()
    {
        return new LicenseSeatAllocationValidator(
            new List<ILicenseSeatAllocationRule>
            {
                new LicenseSeatAllocationRequiredSourceRule("license seat allocation needs an entry"),
                new LicenseSeatAllocationCodePrefixRule("REF"),
                new LicenseSeatAllocationCapacityRangeRule(1000, 86400),
                new LicenseSeatAllocationRequiredCodeRule("license seat allocation needs an entry"),
            });
    }
}
