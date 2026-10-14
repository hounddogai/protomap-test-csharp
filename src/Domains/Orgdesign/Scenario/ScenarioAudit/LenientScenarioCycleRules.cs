namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>A rule a lenient scenario cycle must pass. Check returns its problems, or none.</summary>
public interface ILenientScenarioCycleRule
{
    List<string> Check(LenientScenarioCycle item);
}

public sealed record LenientScenarioCycleRequiredLabelRule(string Message) : ILenientScenarioCycleRule
{
    public List<string> Check(LenientScenarioCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioCycleOwnerIdLengthRule(int MaxLength) : ILenientScenarioCycleRule
{
    public List<string> Check(LenientScenarioCycle item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioCycleLabelLengthRule(int MaxLength) : ILenientScenarioCycleRule
{
    public List<string> Check(LenientScenarioCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioCycleRequiredOwnerIdRule(string Message) : ILenientScenarioCycleRule
{
    public List<string> Check(LenientScenarioCycle item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient scenario cycle.</summary>
public sealed record LenientScenarioCycleValidator(List<ILenientScenarioCycleRule> Rules)
{
    public List<string> Validate(LenientScenarioCycle item)
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

    public void RequireValid(LenientScenarioCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientScenarioCycle> items)
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

/// <summary>Functions of lenient scenario cycle rules.</summary>
public static class LenientScenarioCycleRules
{
    public static LenientScenarioCycleValidator DefaultLenientScenarioCycleValidator()
    {
        return new LenientScenarioCycleValidator(
            new List<ILenientScenarioCycleRule>
            {
                new LenientScenarioCycleRequiredLabelRule("lenient scenario cycle needs this field"),
                new LenientScenarioCycleOwnerIdLengthRule(255),
                new LenientScenarioCycleLabelLengthRule(3600),
                new LenientScenarioCycleRequiredOwnerIdRule("lenient scenario cycle needs a value"),
            });
    }
}
