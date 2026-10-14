namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band adjustment must pass. Check returns its problems, or none.</summary>
public interface IPayBandAdjustmentRule
{
    List<string> Check(PayBandAdjustment item);
}

public sealed record PayBandAdjustmentRequiredExternalRefRule(string Message) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAdjustmentAllowedCodeRule(List<string> Allowed) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAdjustmentFteRangeRule(double Min, double Max) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAdjustmentRequiredCodeRule(string Message) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAdjustmentCodePrefixRule(string Prefix) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAdjustmentRateRangeRule(double Min, double Max) : IPayBandAdjustmentRule
{
    public List<string> Check(PayBandAdjustment item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band adjustment.</summary>
public sealed record PayBandAdjustmentValidator(List<IPayBandAdjustmentRule> Rules)
{
    public List<string> Validate(PayBandAdjustment item)
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

    public void RequireValid(PayBandAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandAdjustment> items)
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

/// <summary>Functions of pay band adjustment rules.</summary>
public static class PayBandAdjustmentRules
{
    public static PayBandAdjustmentValidator DefaultPayBandAdjustmentValidator()
    {
        return new PayBandAdjustmentValidator(
            new List<IPayBandAdjustmentRule>
            {
                new PayBandAdjustmentRequiredExternalRefRule("pay band adjustment needs a value"),
                new PayBandAdjustmentAllowedCodeRule(new List<string>()),
                new PayBandAdjustmentFteRangeRule(7.9, 2507.2),
                new PayBandAdjustmentRequiredCodeRule("pay band adjustment needs an entry"),
                new PayBandAdjustmentCodePrefixRule("ACM"),
                new PayBandAdjustmentRateRangeRule(9.3, 1150.6),
            });
    }
}
