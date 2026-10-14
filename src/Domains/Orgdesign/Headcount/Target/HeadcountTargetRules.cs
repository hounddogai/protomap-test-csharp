namespace Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>A rule a headcount target must pass. Check returns its problems, or none.</summary>
public interface IHeadcountTargetRule
{
    List<string> Check(HeadcountTarget item);
}

public sealed record HeadcountTargetRequiredCodeRule(string Message) : IHeadcountTargetRule
{
    public List<string> Check(HeadcountTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTargetCostRangeRule(double Min, double Max) : IHeadcountTargetRule
{
    public List<string> Check(HeadcountTarget item)
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

public sealed record HeadcountTargetRequiredLabelRule(string Message) : IHeadcountTargetRule
{
    public List<string> Check(HeadcountTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTargetRequiredDepartmentRule(string Message) : IHeadcountTargetRule
{
    public List<string> Check(HeadcountTarget item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount target.</summary>
public sealed record HeadcountTargetValidator(List<IHeadcountTargetRule> Rules)
{
    public List<string> Validate(HeadcountTarget item)
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

    public void RequireValid(HeadcountTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountTarget> items)
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

/// <summary>Functions of headcount target rules.</summary>
public static class HeadcountTargetRules
{
    public static HeadcountTargetValidator DefaultHeadcountTargetValidator()
    {
        return new HeadcountTargetValidator(
            new List<IHeadcountTargetRule>
            {
                new HeadcountTargetRequiredCodeRule("headcount target needs a value"),
                new HeadcountTargetCostRangeRule(8.5, 1506.4),
                new HeadcountTargetRequiredLabelRule("headcount target needs a value"),
                new HeadcountTargetRequiredDepartmentRule("headcount target needs a value"),
            });
    }
}
