namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>A rule a retro pay allocation must pass. Check returns its problems, or none.</summary>
public interface IRetroPayAllocationRule
{
    List<string> Check(RetroPayAllocation item);
}

public sealed record RetroPayAllocationCodePrefixRule(string Prefix) : IRetroPayAllocationRule
{
    public List<string> Check(RetroPayAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAllocationRequiredLabelRule(string Message) : IRetroPayAllocationRule
{
    public List<string> Check(RetroPayAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAllocationEndsAtRangeRule(int Min, int Max) : IRetroPayAllocationRule
{
    public List<string> Check(RetroPayAllocation item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a retro pay allocation.</summary>
public sealed record RetroPayAllocationValidator(List<IRetroPayAllocationRule> Rules)
{
    public List<string> Validate(RetroPayAllocation item)
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

    public void RequireValid(RetroPayAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayAllocation> items)
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

/// <summary>Functions of retro pay allocation rules.</summary>
public static class RetroPayAllocationRules
{
    public static RetroPayAllocationValidator DefaultRetroPayAllocationValidator()
    {
        return new RetroPayAllocationValidator(
            new List<IRetroPayAllocationRule>
            {
                new RetroPayAllocationCodePrefixRule("X"),
                new RetroPayAllocationRequiredLabelRule("retro pay allocation needs an entry"),
                new RetroPayAllocationEndsAtRangeRule(3600, 86400),
            });
    }
}
