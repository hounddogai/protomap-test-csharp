namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>A rule a fallback exception cycle must pass. Check returns its problems, or none.</summary>
public interface IFallbackExceptionCycleRule
{
    List<string> Check(FallbackExceptionCycle item);
}

public sealed record FallbackExceptionCycleRequiredCodeRule(string Message) : IFallbackExceptionCycleRule
{
    public List<string> Check(FallbackExceptionCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionCycleRequiredLabelRule(string Message) : IFallbackExceptionCycleRule
{
    public List<string> Check(FallbackExceptionCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionCycleLabelLengthRule(int MaxLength) : IFallbackExceptionCycleRule
{
    public List<string> Check(FallbackExceptionCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionCycleRequiredContactEmailRule(string Message) : IFallbackExceptionCycleRule
{
    public List<string> Check(FallbackExceptionCycle item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback exception cycle.</summary>
public sealed record FallbackExceptionCycleValidator(List<IFallbackExceptionCycleRule> Rules)
{
    public List<string> Validate(FallbackExceptionCycle item)
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

    public void RequireValid(FallbackExceptionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackExceptionCycle> items)
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

/// <summary>Functions of fallback exception cycle rules.</summary>
public static class FallbackExceptionCycleRules
{
    public static FallbackExceptionCycleValidator DefaultFallbackExceptionCycleValidator()
    {
        return new FallbackExceptionCycleValidator(
            new List<IFallbackExceptionCycleRule>
            {
                new FallbackExceptionCycleRequiredCodeRule("fallback exception cycle needs a value"),
                new FallbackExceptionCycleRequiredLabelRule("fallback exception cycle needs this field"),
                new FallbackExceptionCycleLabelLengthRule(255),
                new FallbackExceptionCycleRequiredContactEmailRule("fallback exception cycle needs a value"),
            });
    }
}
