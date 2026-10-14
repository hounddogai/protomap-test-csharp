namespace Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>A rule a pay run allocation must pass. Check returns its problems, or none.</summary>
public interface IPayRunAllocationRule
{
    List<string> Check(PayRunAllocation item);
}

public sealed record PayRunAllocationCodePrefixRule(string Prefix) : IPayRunAllocationRule
{
    public List<string> Check(PayRunAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAllocationScoreRangeRule(double Min, double Max) : IPayRunAllocationRule
{
    public List<string> Check(PayRunAllocation item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunAllocationRequiredManagerIdRule(string Message) : IPayRunAllocationRule
{
    public List<string> Check(PayRunAllocation item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run allocation.</summary>
public sealed record PayRunAllocationValidator(List<IPayRunAllocationRule> Rules)
{
    public List<string> Validate(PayRunAllocation item)
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

    public void RequireValid(PayRunAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunAllocation> items)
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

/// <summary>Functions of pay run allocation rules.</summary>
public static class PayRunAllocationRules
{
    public static PayRunAllocationValidator DefaultPayRunAllocationValidator()
    {
        return new PayRunAllocationValidator(
            new List<IPayRunAllocationRule>
            {
                new PayRunAllocationCodePrefixRule("X"),
                new PayRunAllocationScoreRangeRule(7.4, 2282.6),
                new PayRunAllocationRequiredManagerIdRule("pay run allocation needs an entry"),
            });
    }
}
