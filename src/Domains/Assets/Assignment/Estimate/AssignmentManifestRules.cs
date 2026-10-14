namespace Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>A rule a assignment manifest must pass. Check returns its problems, or none.</summary>
public interface IAssignmentManifestRule
{
    List<string> Check(AssignmentManifest item);
}

public sealed record AssignmentManifestThresholdRangeRule(double Min, double Max) : IAssignmentManifestRule
{
    public List<string> Check(AssignmentManifest item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentManifestFteRangeRule(double Min, double Max) : IAssignmentManifestRule
{
    public List<string> Check(AssignmentManifest item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentManifestHoursRangeRule(double Min, double Max) : IAssignmentManifestRule
{
    public List<string> Check(AssignmentManifest item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentManifestRequiredNameRule(string Message) : IAssignmentManifestRule
{
    public List<string> Check(AssignmentManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentManifestRequiredSalaryBandRule(string Message) : IAssignmentManifestRule
{
    public List<string> Check(AssignmentManifest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment manifest.</summary>
public sealed record AssignmentManifestValidator(List<IAssignmentManifestRule> Rules)
{
    public List<string> Validate(AssignmentManifest item)
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

    public void RequireValid(AssignmentManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentManifest> items)
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

/// <summary>Functions of assignment manifest rules.</summary>
public static class AssignmentManifestRules
{
    public static AssignmentManifestValidator DefaultAssignmentManifestValidator()
    {
        return new AssignmentManifestValidator(
            new List<IAssignmentManifestRule>
            {
                new AssignmentManifestThresholdRangeRule(8.2, 3120.5),
                new AssignmentManifestFteRangeRule(6.4, 780.6),
                new AssignmentManifestHoursRangeRule(0.8, 1684.3),
                new AssignmentManifestRequiredNameRule("assignment manifest needs an entry"),
                new AssignmentManifestRequiredSalaryBandRule("assignment manifest needs this field"),
            });
    }
}
