namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>A rule a cost model threshold must pass. Check returns its problems, or none.</summary>
public interface ICostModelThresholdRule
{
    List<string> Check(CostModelThreshold item);
}

public sealed record CostModelThresholdRequiredNotesRule(string Message) : ICostModelThresholdRule
{
    public List<string> Check(CostModelThreshold item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelThresholdPriorityRangeRule(int Min, int Max) : ICostModelThresholdRule
{
    public List<string> Check(CostModelThreshold item)
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

public sealed record CostModelThresholdRegionLengthRule(int MaxLength) : ICostModelThresholdRule
{
    public List<string> Check(CostModelThreshold item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelThresholdRequiredNameRule(string Message) : ICostModelThresholdRule
{
    public List<string> Check(CostModelThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model threshold.</summary>
public sealed record CostModelThresholdValidator(List<ICostModelThresholdRule> Rules)
{
    public List<string> Validate(CostModelThreshold item)
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

    public void RequireValid(CostModelThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelThreshold> items)
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

/// <summary>Functions of cost model threshold rules.</summary>
public static class CostModelThresholdRules
{
    public static CostModelThresholdValidator DefaultCostModelThresholdValidator()
    {
        return new CostModelThresholdValidator(
            new List<ICostModelThresholdRule>
            {
                new CostModelThresholdRequiredNotesRule("cost model threshold needs an entry"),
                new CostModelThresholdPriorityRangeRule(64, 1000),
                new CostModelThresholdRegionLengthRule(32),
                new CostModelThresholdRequiredNameRule("cost model threshold needs an entry"),
            });
    }
}
