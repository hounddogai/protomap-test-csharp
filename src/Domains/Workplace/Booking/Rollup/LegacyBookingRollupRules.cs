namespace Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>A rule a legacy booking rollup must pass. Check returns its problems, or none.</summary>
public interface ILegacyBookingRollupRule
{
    List<string> Check(LegacyBookingRollup item);
}

public sealed record LegacyBookingRollupScoreRangeRule(double Min, double Max) : ILegacyBookingRollupRule
{
    public List<string> Check(LegacyBookingRollup item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingRollupCreatedAtRangeRule(int Min, int Max) : ILegacyBookingRollupRule
{
    public List<string> Check(LegacyBookingRollup item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyBookingRollupRequiredLocationRule(string Message) : ILegacyBookingRollupRule
{
    public List<string> Check(LegacyBookingRollup item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingRollupBadgeLengthRule(int MaxLength) : ILegacyBookingRollupRule
{
    public List<string> Check(LegacyBookingRollup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingRollupRequiredCodeRule(string Message) : ILegacyBookingRollupRule
{
    public List<string> Check(LegacyBookingRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy booking rollup.</summary>
public sealed record LegacyBookingRollupValidator(List<ILegacyBookingRollupRule> Rules)
{
    public List<string> Validate(LegacyBookingRollup item)
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

    public void RequireValid(LegacyBookingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBookingRollup> items)
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

/// <summary>Functions of legacy booking rollup rules.</summary>
public static class LegacyBookingRollupRules
{
    public static LegacyBookingRollupValidator DefaultLegacyBookingRollupValidator()
    {
        return new LegacyBookingRollupValidator(
            new List<ILegacyBookingRollupRule>
            {
                new LegacyBookingRollupScoreRangeRule(2.2, 2048.2),
                new LegacyBookingRollupCreatedAtRangeRule(16, 1000),
                new LegacyBookingRollupRequiredLocationRule("legacy booking rollup needs an entry"),
                new LegacyBookingRollupBadgeLengthRule(120),
                new LegacyBookingRollupRequiredCodeRule("legacy booking rollup needs this field"),
            });
    }
}
