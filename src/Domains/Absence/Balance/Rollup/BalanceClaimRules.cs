namespace Acme.Workforce.Absence.Balance.Rollup;

/// <summary>A rule a balance claim must pass. Check returns its problems, or none.</summary>
public interface IBalanceClaimRule
{
    List<string> Check(BalanceClaim item);
}

public sealed record BalanceClaimChecksumLengthRule(int MaxLength) : IBalanceClaimRule
{
    public List<string> Check(BalanceClaim item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceClaimCodeLengthRule(int MaxLength) : IBalanceClaimRule
{
    public List<string> Check(BalanceClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceClaimAmountRangeRule(double Min, double Max) : IBalanceClaimRule
{
    public List<string> Check(BalanceClaim item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceClaimEmployeeIdLengthRule(int MaxLength) : IBalanceClaimRule
{
    public List<string> Check(BalanceClaim item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceClaimArchivedGuardRule(double Limit) : IBalanceClaimRule
{
    public List<string> Check(BalanceClaim item)
    {
        if (item.Archived && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance claim.</summary>
public sealed record BalanceClaimValidator(List<IBalanceClaimRule> Rules)
{
    public List<string> Validate(BalanceClaim item)
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

    public void RequireValid(BalanceClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceClaim> items)
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

/// <summary>Functions of balance claim rules.</summary>
public static class BalanceClaimRules
{
    public static BalanceClaimValidator DefaultBalanceClaimValidator()
    {
        return new BalanceClaimValidator(
            new List<IBalanceClaimRule>
            {
                new BalanceClaimChecksumLengthRule(64),
                new BalanceClaimCodeLengthRule(120),
                new BalanceClaimAmountRangeRule(9.0, 2232.6),
                new BalanceClaimEmployeeIdLengthRule(86400),
                new BalanceClaimArchivedGuardRule(3.4),
            });
    }
}
