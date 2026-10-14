namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model override must pass. Check returns its problems, or none.</summary>
public interface ICostModelOverrideRule
{
    List<string> Check(CostModelOverride item);
}

public sealed record CostModelOverrideDepartmentLengthRule(int MaxLength) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelOverrideAllowedCodeRule(List<string> Allowed) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record CostModelOverrideRequiredTitleRule(string Message) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelOverrideCodeLengthRule(int MaxLength) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelOverrideWindowRule(int MaxSpan) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record CostModelOverrideRequiredLocaleRule(string Message) : ICostModelOverrideRule
{
    public List<string> Check(CostModelOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model override.</summary>
public sealed record CostModelOverrideValidator(List<ICostModelOverrideRule> Rules)
{
    public List<string> Validate(CostModelOverride item)
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

    public void RequireValid(CostModelOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelOverride> items)
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

/// <summary>Functions of cost model override rules.</summary>
public static class CostModelOverrideRules
{
    public static CostModelOverrideValidator DefaultCostModelOverrideValidator()
    {
        return new CostModelOverrideValidator(
            new List<ICostModelOverrideRule>
            {
                new CostModelOverrideDepartmentLengthRule(120),
                new CostModelOverrideAllowedCodeRule(new List<string> { "capped" }),
                new CostModelOverrideRequiredTitleRule("cost model override needs this field"),
                new CostModelOverrideCodeLengthRule(86400),
                new CostModelOverrideWindowRule(32),
                new CostModelOverrideRequiredLocaleRule("cost model override needs an entry"),
            });
    }
}
