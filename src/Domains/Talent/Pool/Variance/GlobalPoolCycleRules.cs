namespace Acme.Workforce.Talent.Pool.Variance;

/// <summary>A rule a global pool cycle must pass. Check returns its problems, or none.</summary>
public interface IGlobalPoolCycleRule
{
    List<string> Check(GlobalPoolCycle item);
}

public sealed record GlobalPoolCycleRequiredCodeRule(string Message) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolCycleRequiredNameRule(string Message) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolCycleCodeLengthRule(int MaxLength) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolCycleWeightRangeRule(double Min, double Max) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolCycleApprovedGuardRule(double Limit) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        if (item.Approved && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolCycleNameLengthRule(int MaxLength) : IGlobalPoolCycleRule
{
    public List<string> Check(GlobalPoolCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global pool cycle.</summary>
public sealed record GlobalPoolCycleValidator(List<IGlobalPoolCycleRule> Rules)
{
    public List<string> Validate(GlobalPoolCycle item)
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

    public void RequireValid(GlobalPoolCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPoolCycle> items)
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

/// <summary>Functions of global pool cycle rules.</summary>
public static class GlobalPoolCycleRules
{
    public static GlobalPoolCycleValidator DefaultGlobalPoolCycleValidator()
    {
        return new GlobalPoolCycleValidator(
            new List<IGlobalPoolCycleRule>
            {
                new GlobalPoolCycleRequiredCodeRule("global pool cycle needs an entry"),
                new GlobalPoolCycleRequiredNameRule("global pool cycle needs this field"),
                new GlobalPoolCycleCodeLengthRule(64),
                new GlobalPoolCycleWeightRangeRule(4.5, 337.1),
                new GlobalPoolCycleApprovedGuardRule(5.0),
                new GlobalPoolCycleNameLengthRule(3600),
            });
    }
}
