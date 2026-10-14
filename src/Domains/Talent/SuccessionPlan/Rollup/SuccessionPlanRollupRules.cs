namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan rollup must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanRollupRule
{
    List<string> Check(SuccessionPlanRollup item);
}

public sealed record SuccessionPlanRollupRequiredCodeRule(string Message) : ISuccessionPlanRollupRule
{
    public List<string> Check(SuccessionPlanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRollupAllowedCodeRule(List<string> Allowed) : ISuccessionPlanRollupRule
{
    public List<string> Check(SuccessionPlanRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRollupRequiredTimezoneRule(string Message) : ISuccessionPlanRollupRule
{
    public List<string> Check(SuccessionPlanRollup item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRollupTeamLengthRule(int MaxLength) : ISuccessionPlanRollupRule
{
    public List<string> Check(SuccessionPlanRollup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRollupLabelLengthRule(int MaxLength) : ISuccessionPlanRollupRule
{
    public List<string> Check(SuccessionPlanRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan rollup.</summary>
public sealed record SuccessionPlanRollupValidator(List<ISuccessionPlanRollupRule> Rules)
{
    public List<string> Validate(SuccessionPlanRollup item)
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

    public void RequireValid(SuccessionPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanRollup> items)
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

/// <summary>Functions of succession plan rollup rules.</summary>
public static class SuccessionPlanRollupRules
{
    public static SuccessionPlanRollupValidator DefaultSuccessionPlanRollupValidator()
    {
        return new SuccessionPlanRollupValidator(
            new List<ISuccessionPlanRollupRule>
            {
                new SuccessionPlanRollupRequiredCodeRule("succession plan rollup needs an entry"),
                new SuccessionPlanRollupAllowedCodeRule(new List<string> { "adaptive", "default" }),
                new SuccessionPlanRollupRequiredTimezoneRule("succession plan rollup needs this field"),
                new SuccessionPlanRollupTeamLengthRule(64),
                new SuccessionPlanRollupLabelLengthRule(1000),
            });
    }
}
