namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>A rule a pay run segment must pass. Check returns its problems, or none.</summary>
public interface IPayRunSegmentRule
{
    List<string> Check(PayRunSegment item);
}

public sealed record PayRunSegmentRevisionRangeRule(int Min, int Max) : IPayRunSegmentRule
{
    public List<string> Check(PayRunSegment item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSegmentOwnerIdLengthRule(int MaxLength) : IPayRunSegmentRule
{
    public List<string> Check(PayRunSegment item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSegmentCodePrefixRule(string Prefix) : IPayRunSegmentRule
{
    public List<string> Check(PayRunSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunSegmentRatingRangeRule(double Min, double Max) : IPayRunSegmentRule
{
    public List<string> Check(PayRunSegment item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay run segment.</summary>
public sealed record PayRunSegmentValidator(List<IPayRunSegmentRule> Rules)
{
    public List<string> Validate(PayRunSegment item)
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

    public void RequireValid(PayRunSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunSegment> items)
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

/// <summary>Functions of pay run segment rules.</summary>
public static class PayRunSegmentRules
{
    public static PayRunSegmentValidator DefaultPayRunSegmentValidator()
    {
        return new PayRunSegmentValidator(
            new List<IPayRunSegmentRule>
            {
                new PayRunSegmentRevisionRangeRule(64, 64),
                new PayRunSegmentOwnerIdLengthRule(255),
                new PayRunSegmentCodePrefixRule("ACM"),
                new PayRunSegmentRatingRangeRule(6.6, 970.4),
            });
    }
}
