namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>A rule a reorg cycle must pass. Check returns its problems, or none.</summary>
public interface IReorgCycleRule
{
    List<string> Check(ReorgCycle item);
}

public sealed record ReorgCycleLabelLengthRule(int MaxLength) : IReorgCycleRule
{
    public List<string> Check(ReorgCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgCycleCodeLengthRule(int MaxLength) : IReorgCycleRule
{
    public List<string> Check(ReorgCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgCycleRequiredCodeRule(string Message) : IReorgCycleRule
{
    public List<string> Check(ReorgCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgCycleRequiredEmployeeIdRule(string Message) : IReorgCycleRule
{
    public List<string> Check(ReorgCycle item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg cycle.</summary>
public sealed record ReorgCycleValidator(List<IReorgCycleRule> Rules)
{
    public List<string> Validate(ReorgCycle item)
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

    public void RequireValid(ReorgCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgCycle> items)
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

/// <summary>Functions of reorg cycle rules.</summary>
public static class ReorgCycleRules
{
    public static ReorgCycleValidator DefaultReorgCycleValidator()
    {
        return new ReorgCycleValidator(
            new List<IReorgCycleRule>
            {
                new ReorgCycleLabelLengthRule(3600),
                new ReorgCycleCodeLengthRule(32),
                new ReorgCycleRequiredCodeRule("reorg cycle needs an entry"),
                new ReorgCycleRequiredEmployeeIdRule("reorg cycle needs an entry"),
            });
    }
}
