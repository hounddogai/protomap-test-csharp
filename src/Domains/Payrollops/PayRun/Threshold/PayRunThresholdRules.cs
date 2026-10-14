namespace Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>A rule a pay run threshold must pass. Check returns its problems, or none.</summary>
public interface IPayRunThresholdRule
{
    List<string> Check(PayRunThreshold item);
}

public sealed record PayRunThresholdCodePrefixRule(string Prefix) : IPayRunThresholdRule
{
    public List<string> Check(PayRunThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunThresholdRequiredCountryRule(string Message) : IPayRunThresholdRule
{
    public List<string> Check(PayRunThreshold item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunThresholdRequiredExternalRefRule(string Message) : IPayRunThresholdRule
{
    public List<string> Check(PayRunThreshold item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunThresholdCodeLengthRule(int MaxLength) : IPayRunThresholdRule
{
    public List<string> Check(PayRunThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunThresholdExternalRefLengthRule(int MaxLength) : IPayRunThresholdRule
{
    public List<string> Check(PayRunThreshold item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run threshold.</summary>
public sealed record PayRunThresholdValidator(List<IPayRunThresholdRule> Rules)
{
    public List<string> Validate(PayRunThreshold item)
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

    public void RequireValid(PayRunThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunThreshold> items)
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

/// <summary>Functions of pay run threshold rules.</summary>
public static class PayRunThresholdRules
{
    public static PayRunThresholdValidator DefaultPayRunThresholdValidator()
    {
        return new PayRunThresholdValidator(
            new List<IPayRunThresholdRule>
            {
                new PayRunThresholdCodePrefixRule("Q"),
                new PayRunThresholdRequiredCountryRule("pay run threshold needs this field"),
                new PayRunThresholdRequiredExternalRefRule("pay run threshold needs this field"),
                new PayRunThresholdCodeLengthRule(86400),
                new PayRunThresholdExternalRefLengthRule(64),
            });
    }
}
