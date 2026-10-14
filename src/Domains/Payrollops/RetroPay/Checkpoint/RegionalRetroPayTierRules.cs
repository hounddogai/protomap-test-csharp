namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional retro pay tier must pass. Check returns its problems, or none.</summary>
public interface IRegionalRetroPayTierRule
{
    List<string> Check(RegionalRetroPayTier item);
}

public sealed record RegionalRetroPayTierEmployeeIdLengthRule(int MaxLength) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRetroPayTierCodeLengthRule(int MaxLength) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRetroPayTierAllowedCodeRule(List<string> Allowed) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRetroPayTierRequiredCodeRule(string Message) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalRetroPayTierCategoryLengthRule(int MaxLength) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRetroPayTierRequiredEmployeeIdRule(string Message) : IRegionalRetroPayTierRule
{
    public List<string> Check(RegionalRetroPayTier item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional retro pay tier.</summary>
public sealed record RegionalRetroPayTierValidator(List<IRegionalRetroPayTierRule> Rules)
{
    public List<string> Validate(RegionalRetroPayTier item)
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

    public void RequireValid(RegionalRetroPayTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalRetroPayTier> items)
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

/// <summary>Functions of regional retro pay tier rules.</summary>
public static class RegionalRetroPayTierRules
{
    public static RegionalRetroPayTierValidator DefaultRegionalRetroPayTierValidator()
    {
        return new RegionalRetroPayTierValidator(
            new List<IRegionalRetroPayTierRule>
            {
                new RegionalRetroPayTierEmployeeIdLengthRule(3600),
                new RegionalRetroPayTierCodeLengthRule(86400),
                new RegionalRetroPayTierAllowedCodeRule(new List<string> { "global", "capped", "strict" }),
                new RegionalRetroPayTierRequiredCodeRule("regional retro pay tier needs a value"),
                new RegionalRetroPayTierCategoryLengthRule(3600),
                new RegionalRetroPayTierRequiredEmployeeIdRule("regional retro pay tier needs an entry"),
            });
    }
}
