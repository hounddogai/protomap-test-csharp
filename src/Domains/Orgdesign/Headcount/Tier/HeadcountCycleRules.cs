namespace Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>A rule a headcount cycle must pass. Check returns its problems, or none.</summary>
public interface IHeadcountCycleRule
{
    List<string> Check(HeadcountCycle item);
}

public sealed record HeadcountCycleBadgeLengthRule(int MaxLength) : IHeadcountCycleRule
{
    public List<string> Check(HeadcountCycle item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCycleRequiredLabelRule(string Message) : IHeadcountCycleRule
{
    public List<string> Check(HeadcountCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountCycleWindowDaysRangeRule(int Min, int Max) : IHeadcountCycleRule
{
    public List<string> Check(HeadcountCycle item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountCycleCostRangeRule(double Min, double Max) : IHeadcountCycleRule
{
    public List<string> Check(HeadcountCycle item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountCycleCodeLengthRule(int MaxLength) : IHeadcountCycleRule
{
    public List<string> Check(HeadcountCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount cycle.</summary>
public sealed record HeadcountCycleValidator(List<IHeadcountCycleRule> Rules)
{
    public List<string> Validate(HeadcountCycle item)
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

    public void RequireValid(HeadcountCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountCycle> items)
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

/// <summary>Functions of headcount cycle rules.</summary>
public static class HeadcountCycleRules
{
    public static HeadcountCycleValidator DefaultHeadcountCycleValidator()
    {
        return new HeadcountCycleValidator(
            new List<IHeadcountCycleRule>
            {
                new HeadcountCycleBadgeLengthRule(86400),
                new HeadcountCycleRequiredLabelRule("headcount cycle needs a value"),
                new HeadcountCycleWindowDaysRangeRule(120, 255),
                new HeadcountCycleCostRangeRule(8.3, 3845.3),
                new HeadcountCycleCodeLengthRule(32),
            });
    }
}
