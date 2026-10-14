namespace Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>A rule a retro pay claim must pass. Check returns its problems, or none.</summary>
public interface IRetroPayClaimRule
{
    List<string> Check(RetroPayClaim item);
}

public sealed record RetroPayClaimRatioRangeRule(double Min, double Max) : IRetroPayClaimRule
{
    public List<string> Check(RetroPayClaim item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayClaimRateRangeRule(double Min, double Max) : IRetroPayClaimRule
{
    public List<string> Check(RetroPayClaim item)
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

public sealed record RetroPayClaimCodePrefixRule(string Prefix) : IRetroPayClaimRule
{
    public List<string> Check(RetroPayClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay claim.</summary>
public sealed record RetroPayClaimValidator(List<IRetroPayClaimRule> Rules)
{
    public List<string> Validate(RetroPayClaim item)
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

    public void RequireValid(RetroPayClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayClaim> items)
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

/// <summary>Functions of retro pay claim rules.</summary>
public static class RetroPayClaimRules
{
    public static RetroPayClaimValidator DefaultRetroPayClaimValidator()
    {
        return new RetroPayClaimValidator(
            new List<IRetroPayClaimRule>
            {
                new RetroPayClaimRatioRangeRule(9.8, 427.9),
                new RetroPayClaimRateRangeRule(9.1, 3712.8),
                new RetroPayClaimCodePrefixRule("REF"),
            });
    }
}
