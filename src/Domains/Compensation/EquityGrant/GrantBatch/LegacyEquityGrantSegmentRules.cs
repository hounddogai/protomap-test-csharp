namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>A rule a legacy equity grant segment must pass. Check returns its problems, or none.</summary>
public interface ILegacyEquityGrantSegmentRule
{
    List<string> Check(LegacyEquityGrantSegment item);
}

public sealed record LegacyEquityGrantSegmentRequiredSalaryBandRule(string Message) : ILegacyEquityGrantSegmentRule
{
    public List<string> Check(LegacyEquityGrantSegment item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyEquityGrantSegmentAmountRangeRule(double Min, double Max) : ILegacyEquityGrantSegmentRule
{
    public List<string> Check(LegacyEquityGrantSegment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyEquityGrantSegmentApprovedGuardRule(int Limit) : ILegacyEquityGrantSegmentRule
{
    public List<string> Check(LegacyEquityGrantSegment item)
    {
        if (item.Approved && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LegacyEquityGrantSegmentCodePrefixRule(string Prefix) : ILegacyEquityGrantSegmentRule
{
    public List<string> Check(LegacyEquityGrantSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy equity grant segment.</summary>
public sealed record LegacyEquityGrantSegmentValidator(List<ILegacyEquityGrantSegmentRule> Rules)
{
    public List<string> Validate(LegacyEquityGrantSegment item)
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

    public void RequireValid(LegacyEquityGrantSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyEquityGrantSegment> items)
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

/// <summary>Functions of legacy equity grant segment rules.</summary>
public static class LegacyEquityGrantSegmentRules
{
    public static LegacyEquityGrantSegmentValidator DefaultLegacyEquityGrantSegmentValidator()
    {
        return new LegacyEquityGrantSegmentValidator(
            new List<ILegacyEquityGrantSegmentRule>
            {
                new LegacyEquityGrantSegmentRequiredSalaryBandRule("legacy equity grant segment needs an entry"),
                new LegacyEquityGrantSegmentAmountRangeRule(1.5, 2603.9),
                new LegacyEquityGrantSegmentApprovedGuardRule(64),
                new LegacyEquityGrantSegmentCodePrefixRule("Q"),
            });
    }
}
