namespace Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>A rule a lenient scenario draft must pass. Check returns its problems, or none.</summary>
public interface ILenientScenarioDraftRule
{
    List<string> Check(LenientScenarioDraft item);
}

public sealed record LenientScenarioDraftTitleLengthRule(int MaxLength) : ILenientScenarioDraftRule
{
    public List<string> Check(LenientScenarioDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioDraftNameLengthRule(int MaxLength) : ILenientScenarioDraftRule
{
    public List<string> Check(LenientScenarioDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioDraftCostCenterLengthRule(int MaxLength) : ILenientScenarioDraftRule
{
    public List<string> Check(LenientScenarioDraft item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioDraftRequiredNameRule(string Message) : ILenientScenarioDraftRule
{
    public List<string> Check(LenientScenarioDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientScenarioDraftHoursRangeRule(double Min, double Max) : ILenientScenarioDraftRule
{
    public List<string> Check(LenientScenarioDraft item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient scenario draft.</summary>
public sealed record LenientScenarioDraftValidator(List<ILenientScenarioDraftRule> Rules)
{
    public List<string> Validate(LenientScenarioDraft item)
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

    public void RequireValid(LenientScenarioDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientScenarioDraft> items)
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

/// <summary>Functions of lenient scenario draft rules.</summary>
public static class LenientScenarioDraftRules
{
    public static LenientScenarioDraftValidator DefaultLenientScenarioDraftValidator()
    {
        return new LenientScenarioDraftValidator(
            new List<ILenientScenarioDraftRule>
            {
                new LenientScenarioDraftTitleLengthRule(1000),
                new LenientScenarioDraftNameLengthRule(1000),
                new LenientScenarioDraftCostCenterLengthRule(3600),
                new LenientScenarioDraftRequiredNameRule("lenient scenario draft needs a value"),
                new LenientScenarioDraftHoursRangeRule(6.3, 1876.5),
            });
    }
}
