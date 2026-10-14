namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched town hall rule must pass. Check returns its problems, or none.</summary>
public interface IBatchedTownHallRuleRule
{
    List<string> Check(BatchedTownHallRule item);
}

public sealed record BatchedTownHallRuleRequiredUnitRule(string Message) : IBatchedTownHallRuleRule
{
    public List<string> Check(BatchedTownHallRule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallRuleRequiredCodeRule(string Message) : IBatchedTownHallRuleRule
{
    public List<string> Check(BatchedTownHallRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallRuleCodeLengthRule(int MaxLength) : IBatchedTownHallRuleRule
{
    public List<string> Check(BatchedTownHallRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallRuleTitleLengthRule(int MaxLength) : IBatchedTownHallRuleRule
{
    public List<string> Check(BatchedTownHallRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallRuleAllowedNameRule(List<string> Allowed) : IBatchedTownHallRuleRule
{
    public List<string> Check(BatchedTownHallRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched town hall rule.</summary>
public sealed record BatchedTownHallRuleValidator(List<IBatchedTownHallRuleRule> Rules)
{
    public List<string> Validate(BatchedTownHallRule item)
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

    public void RequireValid(BatchedTownHallRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTownHallRule> items)
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

/// <summary>Functions of batched town hall rule rules.</summary>
public static class BatchedTownHallRuleRules
{
    public static BatchedTownHallRuleValidator DefaultBatchedTownHallRuleValidator()
    {
        return new BatchedTownHallRuleValidator(
            new List<IBatchedTownHallRuleRule>
            {
                new BatchedTownHallRuleRequiredUnitRule("batched town hall rule needs a value"),
                new BatchedTownHallRuleRequiredCodeRule("batched town hall rule needs a value"),
                new BatchedTownHallRuleCodeLengthRule(255),
                new BatchedTownHallRuleTitleLengthRule(3600),
                new BatchedTownHallRuleAllowedNameRule(new List<string> { "strict" }),
            });
    }
}
