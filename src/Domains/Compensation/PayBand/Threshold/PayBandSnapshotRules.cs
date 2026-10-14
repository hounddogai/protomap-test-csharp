namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band snapshot must pass. Check returns its problems, or none.</summary>
public interface IPayBandSnapshotRule
{
    List<string> Check(PayBandSnapshot item);
}

public sealed record PayBandSnapshotAllowedCodeRule(List<string> Allowed) : IPayBandSnapshotRule
{
    public List<string> Check(PayBandSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PayBandSnapshotRequiredBadgeRule(string Message) : IPayBandSnapshotRule
{
    public List<string> Check(PayBandSnapshot item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandSnapshotCodePrefixRule(string Prefix) : IPayBandSnapshotRule
{
    public List<string> Check(PayBandSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band snapshot.</summary>
public sealed record PayBandSnapshotValidator(List<IPayBandSnapshotRule> Rules)
{
    public List<string> Validate(PayBandSnapshot item)
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

    public void RequireValid(PayBandSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandSnapshot> items)
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

/// <summary>Functions of pay band snapshot rules.</summary>
public static class PayBandSnapshotRules
{
    public static PayBandSnapshotValidator DefaultPayBandSnapshotValidator()
    {
        return new PayBandSnapshotValidator(
            new List<IPayBandSnapshotRule>
            {
                new PayBandSnapshotAllowedCodeRule(new List<string> { "fallback", "capped", "adaptive" }),
                new PayBandSnapshotRequiredBadgeRule("pay band snapshot needs this field"),
                new PayBandSnapshotCodePrefixRule("ACM"),
            });
    }
}
