namespace Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>A rule a batched skill profile audit must pass. Check returns its problems, or none.</summary>
public interface IBatchedSkillProfileAuditRule
{
    List<string> Check(BatchedSkillProfileAudit item);
}

public sealed record BatchedSkillProfileAuditNameLengthRule(int MaxLength) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileAuditCodeLengthRule(int MaxLength) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileAuditRequiredNameRule(string Message) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileAuditFloorValueRangeRule(double Min, double Max) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedSkillProfileAuditCodePrefixRule(string Prefix) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileAuditBudgetRangeRule(double Min, double Max) : IBatchedSkillProfileAuditRule
{
    public List<string> Check(BatchedSkillProfileAudit item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched skill profile audit.</summary>
public sealed record BatchedSkillProfileAuditValidator(List<IBatchedSkillProfileAuditRule> Rules)
{
    public List<string> Validate(BatchedSkillProfileAudit item)
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

    public void RequireValid(BatchedSkillProfileAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSkillProfileAudit> items)
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

/// <summary>Functions of batched skill profile audit rules.</summary>
public static class BatchedSkillProfileAuditRules
{
    public static BatchedSkillProfileAuditValidator DefaultBatchedSkillProfileAuditValidator()
    {
        return new BatchedSkillProfileAuditValidator(
            new List<IBatchedSkillProfileAuditRule>
            {
                new BatchedSkillProfileAuditNameLengthRule(64),
                new BatchedSkillProfileAuditCodeLengthRule(80),
                new BatchedSkillProfileAuditRequiredNameRule("batched skill profile audit needs an entry"),
                new BatchedSkillProfileAuditFloorValueRangeRule(6.6, 539.2),
                new BatchedSkillProfileAuditCodePrefixRule("OPS"),
                new BatchedSkillProfileAuditBudgetRangeRule(7.5, 1441.2),
            });
    }
}
