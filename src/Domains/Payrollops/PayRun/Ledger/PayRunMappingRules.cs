namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run mapping must pass. Check returns its problems, or none.</summary>
public interface IPayRunMappingRule
{
    List<string> Check(PayRunMapping item);
}

public sealed record PayRunMappingAllowedPhoneRule(List<string> Allowed) : IPayRunMappingRule
{
    public List<string> Check(PayRunMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMappingNameLengthRule(int MaxLength) : IPayRunMappingRule
{
    public List<string> Check(PayRunMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMappingMinimumRangeRule(double Min, double Max) : IPayRunMappingRule
{
    public List<string> Check(PayRunMapping item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunMappingApprovedGuardRule(double Limit) : IPayRunMappingRule
{
    public List<string> Check(PayRunMapping item)
    {
        if (item.Approved && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMappingCodePrefixRule(string Prefix) : IPayRunMappingRule
{
    public List<string> Check(PayRunMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run mapping.</summary>
public sealed record PayRunMappingValidator(List<IPayRunMappingRule> Rules)
{
    public List<string> Validate(PayRunMapping item)
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

    public void RequireValid(PayRunMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunMapping> items)
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

/// <summary>Functions of pay run mapping rules.</summary>
public static class PayRunMappingRules
{
    public static PayRunMappingValidator DefaultPayRunMappingValidator()
    {
        return new PayRunMappingValidator(
            new List<IPayRunMappingRule>
            {
                new PayRunMappingAllowedPhoneRule(new List<string> { "pending" }),
                new PayRunMappingNameLengthRule(32),
                new PayRunMappingMinimumRangeRule(3.2, 750.4),
                new PayRunMappingApprovedGuardRule(3.1),
                new PayRunMappingCodePrefixRule("Q"),
            });
    }
}
