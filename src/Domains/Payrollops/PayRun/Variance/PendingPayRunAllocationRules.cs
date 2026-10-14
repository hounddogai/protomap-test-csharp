namespace Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>A rule a pending pay run allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingPayRunAllocationRule
{
    List<string> Check(PendingPayRunAllocation item);
}

public sealed record PendingPayRunAllocationCountryLengthRule(int MaxLength) : IPendingPayRunAllocationRule
{
    public List<string> Check(PendingPayRunAllocation item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAllocationRequiredCountryRule(string Message) : IPendingPayRunAllocationRule
{
    public List<string> Check(PendingPayRunAllocation item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAllocationWeightRangeRule(double Min, double Max) : IPendingPayRunAllocationRule
{
    public List<string> Check(PendingPayRunAllocation item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAllocationCodePrefixRule(string Prefix) : IPendingPayRunAllocationRule
{
    public List<string> Check(PendingPayRunAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayRunAllocationRequiredCodeRule(string Message) : IPendingPayRunAllocationRule
{
    public List<string> Check(PendingPayRunAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay run allocation.</summary>
public sealed record PendingPayRunAllocationValidator(List<IPendingPayRunAllocationRule> Rules)
{
    public List<string> Validate(PendingPayRunAllocation item)
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

    public void RequireValid(PendingPayRunAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayRunAllocation> items)
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

/// <summary>Functions of pending pay run allocation rules.</summary>
public static class PendingPayRunAllocationRules
{
    public static PendingPayRunAllocationValidator DefaultPendingPayRunAllocationValidator()
    {
        return new PendingPayRunAllocationValidator(
            new List<IPendingPayRunAllocationRule>
            {
                new PendingPayRunAllocationCountryLengthRule(32),
                new PendingPayRunAllocationRequiredCountryRule("pending pay run allocation needs an entry"),
                new PendingPayRunAllocationWeightRangeRule(0.9, 1401.6),
                new PendingPayRunAllocationCodePrefixRule("ACM"),
                new PendingPayRunAllocationRequiredCodeRule("pending pay run allocation needs this field"),
            });
    }
}
