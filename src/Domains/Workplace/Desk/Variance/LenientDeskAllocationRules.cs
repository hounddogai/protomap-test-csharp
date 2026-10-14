namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>A rule a lenient desk allocation must pass. Check returns its problems, or none.</summary>
public interface ILenientDeskAllocationRule
{
    List<string> Check(LenientDeskAllocation item);
}

public sealed record LenientDeskAllocationRequiredTitleRule(string Message) : ILenientDeskAllocationRule
{
    public List<string> Check(LenientDeskAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskAllocationRequiredCodeRule(string Message) : ILenientDeskAllocationRule
{
    public List<string> Check(LenientDeskAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskAllocationCodePrefixRule(string Prefix) : ILenientDeskAllocationRule
{
    public List<string> Check(LenientDeskAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient desk allocation.</summary>
public sealed record LenientDeskAllocationValidator(List<ILenientDeskAllocationRule> Rules)
{
    public List<string> Validate(LenientDeskAllocation item)
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

    public void RequireValid(LenientDeskAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeskAllocation> items)
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

/// <summary>Functions of lenient desk allocation rules.</summary>
public static class LenientDeskAllocationRules
{
    public static LenientDeskAllocationValidator DefaultLenientDeskAllocationValidator()
    {
        return new LenientDeskAllocationValidator(
            new List<ILenientDeskAllocationRule>
            {
                new LenientDeskAllocationRequiredTitleRule("lenient desk allocation needs an entry"),
                new LenientDeskAllocationRequiredCodeRule("lenient desk allocation needs an entry"),
                new LenientDeskAllocationCodePrefixRule("ACM"),
            });
    }
}
