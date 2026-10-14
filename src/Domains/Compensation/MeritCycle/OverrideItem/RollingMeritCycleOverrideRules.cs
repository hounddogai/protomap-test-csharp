namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling merit cycle override must pass. Check returns its problems, or none.</summary>
public interface IRollingMeritCycleOverrideRule
{
    List<string> Check(RollingMeritCycleOverride item);
}

public sealed record RollingMeritCycleOverrideAllowedNameRule(List<string> Allowed) : IRollingMeritCycleOverrideRule
{
    public List<string> Check(RollingMeritCycleOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleOverrideCodeLengthRule(int MaxLength) : IRollingMeritCycleOverrideRule
{
    public List<string> Check(RollingMeritCycleOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleOverrideRequiredPhoneRule(string Message) : IRollingMeritCycleOverrideRule
{
    public List<string> Check(RollingMeritCycleOverride item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling merit cycle override.</summary>
public sealed record RollingMeritCycleOverrideValidator(List<IRollingMeritCycleOverrideRule> Rules)
{
    public List<string> Validate(RollingMeritCycleOverride item)
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

    public void RequireValid(RollingMeritCycleOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMeritCycleOverride> items)
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

/// <summary>Functions of rolling merit cycle override rules.</summary>
public static class RollingMeritCycleOverrideRules
{
    public static RollingMeritCycleOverrideValidator DefaultRollingMeritCycleOverrideValidator()
    {
        return new RollingMeritCycleOverrideValidator(
            new List<IRollingMeritCycleOverrideRule>
            {
                new RollingMeritCycleOverrideAllowedNameRule(new List<string>()),
                new RollingMeritCycleOverrideCodeLengthRule(86400),
                new RollingMeritCycleOverrideRequiredPhoneRule("rolling merit cycle override needs this field"),
            });
    }
}
