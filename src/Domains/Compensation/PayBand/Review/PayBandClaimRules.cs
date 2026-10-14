namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band claim must pass. Check returns its problems, or none.</summary>
public interface IPayBandClaimRule
{
    List<string> Check(PayBandClaim item);
}

public sealed record PayBandClaimRequiredLabelRule(string Message) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandClaimAllowedCodeRule(List<string> Allowed) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PayBandClaimCodeLengthRule(int MaxLength) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandClaimCodePrefixRule(string Prefix) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandClaimMinimumRangeRule(double Min, double Max) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
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

public sealed record PayBandClaimRequiredOwnerIdRule(string Message) : IPayBandClaimRule
{
    public List<string> Check(PayBandClaim item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band claim.</summary>
public sealed record PayBandClaimValidator(List<IPayBandClaimRule> Rules)
{
    public List<string> Validate(PayBandClaim item)
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

    public void RequireValid(PayBandClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandClaim> items)
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

/// <summary>Functions of pay band claim rules.</summary>
public static class PayBandClaimRules
{
    public static PayBandClaimValidator DefaultPayBandClaimValidator()
    {
        return new PayBandClaimValidator(
            new List<IPayBandClaimRule>
            {
                new PayBandClaimRequiredLabelRule("pay band claim needs an entry"),
                new PayBandClaimAllowedCodeRule(new List<string> { "scheduled", "default" }),
                new PayBandClaimCodeLengthRule(64),
                new PayBandClaimCodePrefixRule("X"),
                new PayBandClaimMinimumRangeRule(0.7, 315.7),
                new PayBandClaimRequiredOwnerIdRule("pay band claim needs a value"),
            });
    }
}
