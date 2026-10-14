namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>A rule a legacy garnishment allocation must pass. Check returns its problems, or none.</summary>
public interface ILegacyGarnishmentAllocationRule
{
    List<string> Check(LegacyGarnishmentAllocation item);
}

public sealed record LegacyGarnishmentAllocationSalaryBandLengthRule(int MaxLength) : ILegacyGarnishmentAllocationRule
{
    public List<string> Check(LegacyGarnishmentAllocation item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentAllocationRequiredCodeRule(string Message) : ILegacyGarnishmentAllocationRule
{
    public List<string> Check(LegacyGarnishmentAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentAllocationCodePrefixRule(string Prefix) : ILegacyGarnishmentAllocationRule
{
    public List<string> Check(LegacyGarnishmentAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentAllocationEmployeeIdLengthRule(int MaxLength) : ILegacyGarnishmentAllocationRule
{
    public List<string> Check(LegacyGarnishmentAllocation item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentAllocationCodeLengthRule(int MaxLength) : ILegacyGarnishmentAllocationRule
{
    public List<string> Check(LegacyGarnishmentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy garnishment allocation.</summary>
public sealed record LegacyGarnishmentAllocationValidator(List<ILegacyGarnishmentAllocationRule> Rules)
{
    public List<string> Validate(LegacyGarnishmentAllocation item)
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

    public void RequireValid(LegacyGarnishmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyGarnishmentAllocation> items)
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

/// <summary>Functions of legacy garnishment allocation rules.</summary>
public static class LegacyGarnishmentAllocationRules
{
    public static LegacyGarnishmentAllocationValidator DefaultLegacyGarnishmentAllocationValidator()
    {
        return new LegacyGarnishmentAllocationValidator(
            new List<ILegacyGarnishmentAllocationRule>
            {
                new LegacyGarnishmentAllocationSalaryBandLengthRule(120),
                new LegacyGarnishmentAllocationRequiredCodeRule("legacy garnishment allocation needs a value"),
                new LegacyGarnishmentAllocationCodePrefixRule("X"),
                new LegacyGarnishmentAllocationEmployeeIdLengthRule(80),
                new LegacyGarnishmentAllocationCodeLengthRule(255),
            });
    }
}
