namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle policy must pass. Check returns its problems, or none.</summary>
public interface IMeritCyclePolicyRule
{
    List<string> Check(MeritCyclePolicy item);
}

public sealed record MeritCyclePolicyRequiredCodeRule(string Message) : IMeritCyclePolicyRule
{
    public List<string> Check(MeritCyclePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCyclePolicyAllowedCodeRule(List<string> Allowed) : IMeritCyclePolicyRule
{
    public List<string> Check(MeritCyclePolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record MeritCyclePolicyTagsLimitRule(int MaxCount, List<string> Banned) : IMeritCyclePolicyRule
{
    public List<string> Check(MeritCyclePolicy item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle policy.</summary>
public sealed record MeritCyclePolicyValidator(List<IMeritCyclePolicyRule> Rules)
{
    public List<string> Validate(MeritCyclePolicy item)
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

    public void RequireValid(MeritCyclePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCyclePolicy> items)
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

/// <summary>Functions of merit cycle policy rules.</summary>
public static class MeritCyclePolicyRules
{
    public static MeritCyclePolicyValidator DefaultMeritCyclePolicyValidator()
    {
        return new MeritCyclePolicyValidator(
            new List<IMeritCyclePolicyRule>
            {
                new MeritCyclePolicyRequiredCodeRule("merit cycle policy needs a value"),
                new MeritCyclePolicyAllowedCodeRule(new List<string> { "adaptive", "weighted", "adaptive" }),
                new MeritCyclePolicyTagsLimitRule(80, new List<string> { "adaptive", "default" }),
            });
    }
}
