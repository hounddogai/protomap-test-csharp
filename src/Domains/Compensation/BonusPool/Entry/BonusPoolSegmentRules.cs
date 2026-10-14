namespace Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>A rule a bonus pool segment must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolSegmentRule
{
    List<string> Check(BonusPoolSegment item);
}

public sealed record BonusPoolSegmentLabelLengthRule(int MaxLength) : IBonusPoolSegmentRule
{
    public List<string> Check(BonusPoolSegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolSegmentRequiredCountryRule(string Message) : IBonusPoolSegmentRule
{
    public List<string> Check(BonusPoolSegment item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolSegmentMaximumRangeRule(double Min, double Max) : IBonusPoolSegmentRule
{
    public List<string> Check(BonusPoolSegment item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolSegmentPriorityRangeRule(int Min, int Max) : IBonusPoolSegmentRule
{
    public List<string> Check(BonusPoolSegment item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolSegmentRequiredLabelRule(string Message) : IBonusPoolSegmentRule
{
    public List<string> Check(BonusPoolSegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool segment.</summary>
public sealed record BonusPoolSegmentValidator(List<IBonusPoolSegmentRule> Rules)
{
    public List<string> Validate(BonusPoolSegment item)
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

    public void RequireValid(BonusPoolSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolSegment> items)
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

/// <summary>Functions of bonus pool segment rules.</summary>
public static class BonusPoolSegmentRules
{
    public static BonusPoolSegmentValidator DefaultBonusPoolSegmentValidator()
    {
        return new BonusPoolSegmentValidator(
            new List<IBonusPoolSegmentRule>
            {
                new BonusPoolSegmentLabelLengthRule(64),
                new BonusPoolSegmentRequiredCountryRule("bonus pool segment needs a value"),
                new BonusPoolSegmentMaximumRangeRule(5.2, 398.2),
                new BonusPoolSegmentPriorityRangeRule(1000, 3600),
                new BonusPoolSegmentRequiredLabelRule("bonus pool segment needs a value"),
            });
    }
}
