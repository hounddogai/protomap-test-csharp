namespace Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>A rule a merit cycle segment must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleSegmentRule
{
    List<string> Check(MeritCycleSegment item);
}

public sealed record MeritCycleSegmentGraceDaysRangeRule(int Min, int Max) : IMeritCycleSegmentRule
{
    public List<string> Check(MeritCycleSegment item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleSegmentRatingRangeRule(double Min, double Max) : IMeritCycleSegmentRule
{
    public List<string> Check(MeritCycleSegment item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSegmentRequiredLocationRule(string Message) : IMeritCycleSegmentRule
{
    public List<string> Check(MeritCycleSegment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle segment.</summary>
public sealed record MeritCycleSegmentValidator(List<IMeritCycleSegmentRule> Rules)
{
    public List<string> Validate(MeritCycleSegment item)
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

    public void RequireValid(MeritCycleSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleSegment> items)
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

/// <summary>Functions of merit cycle segment rules.</summary>
public static class MeritCycleSegmentRules
{
    public static MeritCycleSegmentValidator DefaultMeritCycleSegmentValidator()
    {
        return new MeritCycleSegmentValidator(
            new List<IMeritCycleSegmentRule>
            {
                new MeritCycleSegmentGraceDaysRangeRule(32, 255),
                new MeritCycleSegmentRatingRangeRule(4.2, 4360.5),
                new MeritCycleSegmentRequiredLocationRule("merit cycle segment needs an entry"),
            });
    }
}
