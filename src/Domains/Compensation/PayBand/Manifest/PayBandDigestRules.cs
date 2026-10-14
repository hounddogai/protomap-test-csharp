namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band digest must pass. Check returns its problems, or none.</summary>
public interface IPayBandDigestRule
{
    List<string> Check(PayBandDigest item);
}

public sealed record PayBandDigestRequiredCodeRule(string Message) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandDigestCodeLengthRule(int MaxLength) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandDigestPinnedGuardRule(double Limit) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        if (item.Pinned && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PayBandDigestRequiredCostCenterRule(string Message) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandDigestRequiredTitleRule(string Message) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandDigestAllowedCostCenterRule(List<string> Allowed) : IPayBandDigestRule
{
    public List<string> Check(PayBandDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band digest.</summary>
public sealed record PayBandDigestValidator(List<IPayBandDigestRule> Rules)
{
    public List<string> Validate(PayBandDigest item)
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

    public void RequireValid(PayBandDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandDigest> items)
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

/// <summary>Functions of pay band digest rules.</summary>
public static class PayBandDigestRules
{
    public static PayBandDigestValidator DefaultPayBandDigestValidator()
    {
        return new PayBandDigestValidator(
            new List<IPayBandDigestRule>
            {
                new PayBandDigestRequiredCodeRule("pay band digest needs this field"),
                new PayBandDigestCodeLengthRule(86400),
                new PayBandDigestPinnedGuardRule(1.5),
                new PayBandDigestRequiredCostCenterRule("pay band digest needs an entry"),
                new PayBandDigestRequiredTitleRule("pay band digest needs a value"),
                new PayBandDigestAllowedCostCenterRule(new List<string> { "regional", "adaptive", "manual" }),
            });
    }
}
