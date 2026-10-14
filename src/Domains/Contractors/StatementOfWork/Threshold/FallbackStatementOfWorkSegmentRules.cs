namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>A rule a fallback statement of work segment must pass. Check returns its problems, or none.</summary>
public interface IFallbackStatementOfWorkSegmentRule
{
    List<string> Check(FallbackStatementOfWorkSegment item);
}

public sealed record FallbackStatementOfWorkSegmentRequiredNameRule(string Message) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackStatementOfWorkSegmentRequiredCodeRule(string Message) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackStatementOfWorkSegmentLimitRangeRule(int Min, int Max) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackStatementOfWorkSegmentVisibleGuardRule(double Limit) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        if (item.Visible && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record FallbackStatementOfWorkSegmentTimezoneLengthRule(int MaxLength) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackStatementOfWorkSegmentCostRangeRule(double Min, double Max) : IFallbackStatementOfWorkSegmentRule
{
    public List<string> Check(FallbackStatementOfWorkSegment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback statement of work segment.</summary>
public sealed record FallbackStatementOfWorkSegmentValidator(List<IFallbackStatementOfWorkSegmentRule> Rules)
{
    public List<string> Validate(FallbackStatementOfWorkSegment item)
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

    public void RequireValid(FallbackStatementOfWorkSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackStatementOfWorkSegment> items)
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

/// <summary>Functions of fallback statement of work segment rules.</summary>
public static class FallbackStatementOfWorkSegmentRules
{
    public static FallbackStatementOfWorkSegmentValidator DefaultFallbackStatementOfWorkSegmentValidator()
    {
        return new FallbackStatementOfWorkSegmentValidator(
            new List<IFallbackStatementOfWorkSegmentRule>
            {
                new FallbackStatementOfWorkSegmentRequiredNameRule(
                    "fallback statement of work segment needs an entry"),
                new FallbackStatementOfWorkSegmentRequiredCodeRule(
                    "fallback statement of work segment needs this field"),
                new FallbackStatementOfWorkSegmentLimitRangeRule(120, 120),
                new FallbackStatementOfWorkSegmentVisibleGuardRule(0.3),
                new FallbackStatementOfWorkSegmentTimezoneLengthRule(86400),
                new FallbackStatementOfWorkSegmentCostRangeRule(2.3, 2436.5),
            });
    }
}
