namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay override must pass. Check returns its problems, or none.</summary>
public interface IRetroPayOverrideRule
{
    List<string> Check(RetroPayOverride item);
}

public sealed record RetroPayOverrideAllowedUnitRule(List<string> Allowed) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayOverrideRequiredCodeRule(string Message) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayOverrideRequiredUnitRule(string Message) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayOverrideRequiredLabelRule(string Message) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayOverrideUnitLengthRule(int MaxLength) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayOverrideLabelLengthRule(int MaxLength) : IRetroPayOverrideRule
{
    public List<string> Check(RetroPayOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay override.</summary>
public sealed record RetroPayOverrideValidator(List<IRetroPayOverrideRule> Rules)
{
    public List<string> Validate(RetroPayOverride item)
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

    public void RequireValid(RetroPayOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayOverride> items)
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

/// <summary>Functions of retro pay override rules.</summary>
public static class RetroPayOverrideRules
{
    public static RetroPayOverrideValidator DefaultRetroPayOverrideValidator()
    {
        return new RetroPayOverrideValidator(
            new List<IRetroPayOverrideRule>
            {
                new RetroPayOverrideAllowedUnitRule(new List<string> { "manual" }),
                new RetroPayOverrideRequiredCodeRule("retro pay override needs an entry"),
                new RetroPayOverrideRequiredUnitRule("retro pay override needs this field"),
                new RetroPayOverrideRequiredLabelRule("retro pay override needs a value"),
                new RetroPayOverrideUnitLengthRule(1000),
                new RetroPayOverrideLabelLengthRule(32),
            });
    }
}
