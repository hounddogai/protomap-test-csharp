namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>A rule a succession plan cycle must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanCycleRule
{
    List<string> Check(SuccessionPlanCycle item);
}

public sealed record SuccessionPlanCycleRequiredLabelRule(string Message) : ISuccessionPlanCycleRule
{
    public List<string> Check(SuccessionPlanCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanCycleCostRangeRule(double Min, double Max) : ISuccessionPlanCycleRule
{
    public List<string> Check(SuccessionPlanCycle item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanCycleSegmentLengthRule(int MaxLength) : ISuccessionPlanCycleRule
{
    public List<string> Check(SuccessionPlanCycle item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanCycleRevisionRangeRule(int Min, int Max) : ISuccessionPlanCycleRule
{
    public List<string> Check(SuccessionPlanCycle item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a succession plan cycle.</summary>
public sealed record SuccessionPlanCycleValidator(List<ISuccessionPlanCycleRule> Rules)
{
    public List<string> Validate(SuccessionPlanCycle item)
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

    public void RequireValid(SuccessionPlanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanCycle> items)
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

/// <summary>Functions of succession plan cycle rules.</summary>
public static class SuccessionPlanCycleRules
{
    public static SuccessionPlanCycleValidator DefaultSuccessionPlanCycleValidator()
    {
        return new SuccessionPlanCycleValidator(
            new List<ISuccessionPlanCycleRule>
            {
                new SuccessionPlanCycleRequiredLabelRule("succession plan cycle needs this field"),
                new SuccessionPlanCycleCostRangeRule(2.6, 4979.7),
                new SuccessionPlanCycleSegmentLengthRule(80),
                new SuccessionPlanCycleRevisionRangeRule(64, 80),
            });
    }
}
