namespace Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>A rule a global headcount checkpoint must pass. Check returns its problems, or none.</summary>
public interface IGlobalHeadcountCheckpointRule
{
    List<string> Check(GlobalHeadcountCheckpoint item);
}

public sealed record GlobalHeadcountCheckpointCostCenterLengthRule(int MaxLength) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalHeadcountCheckpointCodeLengthRule(int MaxLength) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalHeadcountCheckpointSkillLengthRule(int MaxLength) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalHeadcountCheckpointLeaveDaysRangeRule(int Min, int Max) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalHeadcountCheckpointRequiredCodeRule(string Message) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalHeadcountCheckpointRequiredTitleRule(string Message) : IGlobalHeadcountCheckpointRule
{
    public List<string> Check(GlobalHeadcountCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global headcount checkpoint.</summary>
public sealed record GlobalHeadcountCheckpointValidator(List<IGlobalHeadcountCheckpointRule> Rules)
{
    public List<string> Validate(GlobalHeadcountCheckpoint item)
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

    public void RequireValid(GlobalHeadcountCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalHeadcountCheckpoint> items)
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

/// <summary>Functions of global headcount checkpoint rules.</summary>
public static class GlobalHeadcountCheckpointRules
{
    public static GlobalHeadcountCheckpointValidator DefaultGlobalHeadcountCheckpointValidator()
    {
        return new GlobalHeadcountCheckpointValidator(
            new List<IGlobalHeadcountCheckpointRule>
            {
                new GlobalHeadcountCheckpointCostCenterLengthRule(120),
                new GlobalHeadcountCheckpointCodeLengthRule(120),
                new GlobalHeadcountCheckpointSkillLengthRule(1000),
                new GlobalHeadcountCheckpointLeaveDaysRangeRule(16, 1000),
                new GlobalHeadcountCheckpointRequiredCodeRule("global headcount checkpoint needs this field"),
                new GlobalHeadcountCheckpointRequiredTitleRule("global headcount checkpoint needs an entry"),
            });
    }
}
