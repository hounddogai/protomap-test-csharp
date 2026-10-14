namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global desk manifest must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeskManifestRule
{
    List<string> Check(GlobalDeskManifest item);
}

public sealed record GlobalDeskManifestPriorityRangeRule(int Min, int Max) : IGlobalDeskManifestRule
{
    public List<string> Check(GlobalDeskManifest item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalDeskManifestCodeLengthRule(int MaxLength) : IGlobalDeskManifestRule
{
    public List<string> Check(GlobalDeskManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskManifestSalaryBandLengthRule(int MaxLength) : IGlobalDeskManifestRule
{
    public List<string> Check(GlobalDeskManifest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskManifestAllowedDepartmentRule(List<string> Allowed) : IGlobalDeskManifestRule
{
    public List<string> Check(GlobalDeskManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskManifestRequiredTitleRule(string Message) : IGlobalDeskManifestRule
{
    public List<string> Check(GlobalDeskManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global desk manifest.</summary>
public sealed record GlobalDeskManifestValidator(List<IGlobalDeskManifestRule> Rules)
{
    public List<string> Validate(GlobalDeskManifest item)
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

    public void RequireValid(GlobalDeskManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeskManifest> items)
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

/// <summary>Functions of global desk manifest rules.</summary>
public static class GlobalDeskManifestRules
{
    public static GlobalDeskManifestValidator DefaultGlobalDeskManifestValidator()
    {
        return new GlobalDeskManifestValidator(
            new List<IGlobalDeskManifestRule>
            {
                new GlobalDeskManifestPriorityRangeRule(64, 80),
                new GlobalDeskManifestCodeLengthRule(1000),
                new GlobalDeskManifestSalaryBandLengthRule(16),
                new GlobalDeskManifestAllowedDepartmentRule(new List<string> { "global" }),
                new GlobalDeskManifestRequiredTitleRule("global desk manifest needs a value"),
            });
    }
}
