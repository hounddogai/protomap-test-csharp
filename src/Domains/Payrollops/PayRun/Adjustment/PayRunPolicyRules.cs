namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>A rule a pay run policy must pass. Check returns its problems, or none.</summary>
public interface IPayRunPolicyRule
{
    List<string> Check(PayRunPolicy item);
}

public sealed record PayRunPolicyRequiredCodeRule(string Message) : IPayRunPolicyRule
{
    public List<string> Check(PayRunPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunPolicyCodePrefixRule(string Prefix) : IPayRunPolicyRule
{
    public List<string> Check(PayRunPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPolicyLeaveDaysRangeRule(int Min, int Max) : IPayRunPolicyRule
{
    public List<string> Check(PayRunPolicy item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run policy.</summary>
public sealed record PayRunPolicyValidator(List<IPayRunPolicyRule> Rules)
{
    public List<string> Validate(PayRunPolicy item)
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

    public void RequireValid(PayRunPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunPolicy> items)
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

/// <summary>Functions of pay run policy rules.</summary>
public static class PayRunPolicyRules
{
    public static PayRunPolicyValidator DefaultPayRunPolicyValidator()
    {
        return new PayRunPolicyValidator(
            new List<IPayRunPolicyRule>
            {
                new PayRunPolicyRequiredCodeRule("pay run policy needs a value"),
                new PayRunPolicyCodePrefixRule("ACM"),
                new PayRunPolicyLeaveDaysRangeRule(120, 255),
            });
    }
}
