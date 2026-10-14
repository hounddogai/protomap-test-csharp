namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>A rule a equity grant segment must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantSegmentRule
{
    List<string> Check(EquityGrantSegment item);
}

public sealed record EquityGrantSegmentCodePrefixRule(string Prefix) : IEquityGrantSegmentRule
{
    public List<string> Check(EquityGrantSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSegmentNameLengthRule(int MaxLength) : IEquityGrantSegmentRule
{
    public List<string> Check(EquityGrantSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSegmentRequiredNameRule(string Message) : IEquityGrantSegmentRule
{
    public List<string> Check(EquityGrantSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant segment.</summary>
public sealed record EquityGrantSegmentValidator(List<IEquityGrantSegmentRule> Rules)
{
    public List<string> Validate(EquityGrantSegment item)
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

    public void RequireValid(EquityGrantSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantSegment> items)
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

/// <summary>Functions of equity grant segment rules.</summary>
public static class EquityGrantSegmentRules
{
    public static EquityGrantSegmentValidator DefaultEquityGrantSegmentValidator()
    {
        return new EquityGrantSegmentValidator(
            new List<IEquityGrantSegmentRule>
            {
                new EquityGrantSegmentCodePrefixRule("ACM"),
                new EquityGrantSegmentNameLengthRule(16),
                new EquityGrantSegmentRequiredNameRule("equity grant segment needs an entry"),
            });
    }
}
