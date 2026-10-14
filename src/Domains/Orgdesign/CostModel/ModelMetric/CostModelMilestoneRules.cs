namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>A rule a cost model milestone must pass. Check returns its problems, or none.</summary>
public interface ICostModelMilestoneRule
{
    List<string> Check(CostModelMilestone item);
}

public sealed record CostModelMilestoneRequiredCodeRule(string Message) : ICostModelMilestoneRule
{
    public List<string> Check(CostModelMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMilestoneCodeLengthRule(int MaxLength) : ICostModelMilestoneRule
{
    public List<string> Check(CostModelMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMilestoneRequiredLabelRule(string Message) : ICostModelMilestoneRule
{
    public List<string> Check(CostModelMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMilestoneOwnerIdLengthRule(int MaxLength) : ICostModelMilestoneRule
{
    public List<string> Check(CostModelMilestone item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model milestone.</summary>
public sealed record CostModelMilestoneValidator(List<ICostModelMilestoneRule> Rules)
{
    public List<string> Validate(CostModelMilestone item)
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

    public void RequireValid(CostModelMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelMilestone> items)
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

/// <summary>Functions of cost model milestone rules.</summary>
public static class CostModelMilestoneRules
{
    public static CostModelMilestoneValidator DefaultCostModelMilestoneValidator()
    {
        return new CostModelMilestoneValidator(
            new List<ICostModelMilestoneRule>
            {
                new CostModelMilestoneRequiredCodeRule("cost model milestone needs an entry"),
                new CostModelMilestoneCodeLengthRule(3600),
                new CostModelMilestoneRequiredLabelRule("cost model milestone needs a value"),
                new CostModelMilestoneOwnerIdLengthRule(120),
            });
    }
}
