namespace Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>A rule a visitor cycle must pass. Check returns its problems, or none.</summary>
public interface IVisitorCycleRule
{
    List<string> Check(VisitorCycle item);
}

public sealed record VisitorCycleLocationLengthRule(int MaxLength) : IVisitorCycleRule
{
    public List<string> Check(VisitorCycle item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorCycleTenureMonthsRangeRule(int Min, int Max) : IVisitorCycleRule
{
    public List<string> Check(VisitorCycle item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorCycleCodeLengthRule(int MaxLength) : IVisitorCycleRule
{
    public List<string> Check(VisitorCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorCycleRequiredLocationRule(string Message) : IVisitorCycleRule
{
    public List<string> Check(VisitorCycle item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorCycleRequiredNameRule(string Message) : IVisitorCycleRule
{
    public List<string> Check(VisitorCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor cycle.</summary>
public sealed record VisitorCycleValidator(List<IVisitorCycleRule> Rules)
{
    public List<string> Validate(VisitorCycle item)
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

    public void RequireValid(VisitorCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorCycle> items)
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

/// <summary>Functions of visitor cycle rules.</summary>
public static class VisitorCycleRules
{
    public static VisitorCycleValidator DefaultVisitorCycleValidator()
    {
        return new VisitorCycleValidator(
            new List<IVisitorCycleRule>
            {
                new VisitorCycleLocationLengthRule(80),
                new VisitorCycleTenureMonthsRangeRule(32, 1000),
                new VisitorCycleCodeLengthRule(3600),
                new VisitorCycleRequiredLocationRule("visitor cycle needs a value"),
                new VisitorCycleRequiredNameRule("visitor cycle needs an entry"),
            });
    }
}
