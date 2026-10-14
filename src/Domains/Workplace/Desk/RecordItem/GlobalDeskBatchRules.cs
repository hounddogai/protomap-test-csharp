namespace Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>A rule a global desk batch must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeskBatchRule
{
    List<string> Check(GlobalDeskBatch item);
}

public sealed record GlobalDeskBatchSkillLengthRule(int MaxLength) : IGlobalDeskBatchRule
{
    public List<string> Check(GlobalDeskBatch item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskBatchRequiredLabelRule(string Message) : IGlobalDeskBatchRule
{
    public List<string> Check(GlobalDeskBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskBatchBudgetRangeRule(double Min, double Max) : IGlobalDeskBatchRule
{
    public List<string> Check(GlobalDeskBatch item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskBatchCodePrefixRule(string Prefix) : IGlobalDeskBatchRule
{
    public List<string> Check(GlobalDeskBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global desk batch.</summary>
public sealed record GlobalDeskBatchValidator(List<IGlobalDeskBatchRule> Rules)
{
    public List<string> Validate(GlobalDeskBatch item)
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

    public void RequireValid(GlobalDeskBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeskBatch> items)
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

/// <summary>Functions of global desk batch rules.</summary>
public static class GlobalDeskBatchRules
{
    public static GlobalDeskBatchValidator DefaultGlobalDeskBatchValidator()
    {
        return new GlobalDeskBatchValidator(
            new List<IGlobalDeskBatchRule>
            {
                new GlobalDeskBatchSkillLengthRule(255),
                new GlobalDeskBatchRequiredLabelRule("global desk batch needs an entry"),
                new GlobalDeskBatchBudgetRangeRule(9.7, 372.6),
                new GlobalDeskBatchCodePrefixRule("X"),
            });
    }
}
