namespace Acme.Workforce.Workplace.Desk.Profile;

/// <summary>A rule a desk rollup must pass. Check returns its problems, or none.</summary>
public interface IDeskRollupRule
{
    List<string> Check(DeskRollup item);
}

public sealed record DeskRollupNameLengthRule(int MaxLength) : IDeskRollupRule
{
    public List<string> Check(DeskRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRollupRequiredSkillRule(string Message) : IDeskRollupRule
{
    public List<string> Check(DeskRollup item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskRollupCodeLengthRule(int MaxLength) : IDeskRollupRule
{
    public List<string> Check(DeskRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk rollup.</summary>
public sealed record DeskRollupValidator(List<IDeskRollupRule> Rules)
{
    public List<string> Validate(DeskRollup item)
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

    public void RequireValid(DeskRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskRollup> items)
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

/// <summary>Functions of desk rollup rules.</summary>
public static class DeskRollupRules
{
    public static DeskRollupValidator DefaultDeskRollupValidator()
    {
        return new DeskRollupValidator(
            new List<IDeskRollupRule>
            {
                new DeskRollupNameLengthRule(86400),
                new DeskRollupRequiredSkillRule("desk rollup needs an entry"),
                new DeskRollupCodeLengthRule(80),
            });
    }
}
