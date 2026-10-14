namespace Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>A rule a fallback visitor cycle must pass. Check returns its problems, or none.</summary>
public interface IFallbackVisitorCycleRule
{
    List<string> Check(FallbackVisitorCycle item);
}

public sealed record FallbackVisitorCycleRequiredJobCodeRule(string Message) : IFallbackVisitorCycleRule
{
    public List<string> Check(FallbackVisitorCycle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackVisitorCycleWindowRule(int MaxSpan) : IFallbackVisitorCycleRule
{
    public List<string> Check(FallbackVisitorCycle item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record FallbackVisitorCycleRequiredCodeRule(string Message) : IFallbackVisitorCycleRule
{
    public List<string> Check(FallbackVisitorCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackVisitorCycleJobCodeLengthRule(int MaxLength) : IFallbackVisitorCycleRule
{
    public List<string> Check(FallbackVisitorCycle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback visitor cycle.</summary>
public sealed record FallbackVisitorCycleValidator(List<IFallbackVisitorCycleRule> Rules)
{
    public List<string> Validate(FallbackVisitorCycle item)
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

    public void RequireValid(FallbackVisitorCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackVisitorCycle> items)
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

/// <summary>Functions of fallback visitor cycle rules.</summary>
public static class FallbackVisitorCycleRules
{
    public static FallbackVisitorCycleValidator DefaultFallbackVisitorCycleValidator()
    {
        return new FallbackVisitorCycleValidator(
            new List<IFallbackVisitorCycleRule>
            {
                new FallbackVisitorCycleRequiredJobCodeRule("fallback visitor cycle needs an entry"),
                new FallbackVisitorCycleWindowRule(1000),
                new FallbackVisitorCycleRequiredCodeRule("fallback visitor cycle needs this field"),
                new FallbackVisitorCycleJobCodeLengthRule(32),
            });
    }
}
