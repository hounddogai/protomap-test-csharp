namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>A rule a lenient succession plan record must pass. Check returns its problems, or none.</summary>
public interface ILenientSuccessionPlanRecordRule
{
    List<string> Check(LenientSuccessionPlanRecord item);
}

public sealed record LenientSuccessionPlanRecordPinnedGuardRule(double Limit) : ILenientSuccessionPlanRecordRule
{
    public List<string> Check(LenientSuccessionPlanRecord item)
    {
        if (item.Pinned && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanRecordCodePrefixRule(string Prefix) : ILenientSuccessionPlanRecordRule
{
    public List<string> Check(LenientSuccessionPlanRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanRecordDepartmentLengthRule(int MaxLength) : ILenientSuccessionPlanRecordRule
{
    public List<string> Check(LenientSuccessionPlanRecord item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanRecordSequenceRangeRule(int Min, int Max) : ILenientSuccessionPlanRecordRule
{
    public List<string> Check(LenientSuccessionPlanRecord item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientSuccessionPlanRecordRatingRangeRule(double Min, double Max) : ILenientSuccessionPlanRecordRule
{
    public List<string> Check(LenientSuccessionPlanRecord item)
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

/// <summary>Runs every rule of a lenient succession plan record.</summary>
public sealed record LenientSuccessionPlanRecordValidator(List<ILenientSuccessionPlanRecordRule> Rules)
{
    public List<string> Validate(LenientSuccessionPlanRecord item)
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

    public void RequireValid(LenientSuccessionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSuccessionPlanRecord> items)
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

/// <summary>Functions of lenient succession plan record rules.</summary>
public static class LenientSuccessionPlanRecordRules
{
    public static LenientSuccessionPlanRecordValidator DefaultLenientSuccessionPlanRecordValidator()
    {
        return new LenientSuccessionPlanRecordValidator(
            new List<ILenientSuccessionPlanRecordRule>
            {
                new LenientSuccessionPlanRecordPinnedGuardRule(0.2),
                new LenientSuccessionPlanRecordCodePrefixRule("Q"),
                new LenientSuccessionPlanRecordDepartmentLengthRule(1000),
                new LenientSuccessionPlanRecordSequenceRangeRule(16, 80),
                new LenientSuccessionPlanRecordRatingRangeRule(6.9, 711.2),
            });
    }
}
