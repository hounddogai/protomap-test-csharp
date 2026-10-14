namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band allocation must pass. Check returns its problems, or none.</summary>
public interface IPayBandAllocationRule
{
    List<string> Check(PayBandAllocation item);
}

public sealed record PayBandAllocationCodeLengthRule(int MaxLength) : IPayBandAllocationRule
{
    public List<string> Check(PayBandAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAllocationCodePrefixRule(string Prefix) : IPayBandAllocationRule
{
    public List<string> Check(PayBandAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAllocationAllowedTitleRule(List<string> Allowed) : IPayBandAllocationRule
{
    public List<string> Check(PayBandAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAllocationRequiredCodeRule(string Message) : IPayBandAllocationRule
{
    public List<string> Check(PayBandAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAllocationTitleLengthRule(int MaxLength) : IPayBandAllocationRule
{
    public List<string> Check(PayBandAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band allocation.</summary>
public sealed record PayBandAllocationValidator(List<IPayBandAllocationRule> Rules)
{
    public List<string> Validate(PayBandAllocation item)
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

    public void RequireValid(PayBandAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandAllocation> items)
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

/// <summary>Functions of pay band allocation rules.</summary>
public static class PayBandAllocationRules
{
    public static PayBandAllocationValidator DefaultPayBandAllocationValidator()
    {
        return new PayBandAllocationValidator(
            new List<IPayBandAllocationRule>
            {
                new PayBandAllocationCodeLengthRule(3600),
                new PayBandAllocationCodePrefixRule("ACM"),
                new PayBandAllocationAllowedTitleRule(new List<string>()),
                new PayBandAllocationRequiredCodeRule("pay band allocation needs a value"),
                new PayBandAllocationTitleLengthRule(3600),
            });
    }
}
