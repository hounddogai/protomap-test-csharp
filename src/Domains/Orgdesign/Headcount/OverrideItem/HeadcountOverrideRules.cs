namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount override must pass. Check returns its problems, or none.</summary>
public interface IHeadcountOverrideRule
{
    List<string> Check(HeadcountOverride item);
}

public sealed record HeadcountOverrideRequiredLabelRule(string Message) : IHeadcountOverrideRule
{
    public List<string> Check(HeadcountOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountOverrideAllowedDescriptionRule(List<string> Allowed) : IHeadcountOverrideRule
{
    public List<string> Check(HeadcountOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountOverrideLimitRangeRule(int Min, int Max) : IHeadcountOverrideRule
{
    public List<string> Check(HeadcountOverride item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountOverrideCodeLengthRule(int MaxLength) : IHeadcountOverrideRule
{
    public List<string> Check(HeadcountOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount override.</summary>
public sealed record HeadcountOverrideValidator(List<IHeadcountOverrideRule> Rules)
{
    public List<string> Validate(HeadcountOverride item)
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

    public void RequireValid(HeadcountOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountOverride> items)
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

/// <summary>Functions of headcount override rules.</summary>
public static class HeadcountOverrideRules
{
    public static HeadcountOverrideValidator DefaultHeadcountOverrideValidator()
    {
        return new HeadcountOverrideValidator(
            new List<IHeadcountOverrideRule>
            {
                new HeadcountOverrideRequiredLabelRule("headcount override needs this field"),
                new HeadcountOverrideAllowedDescriptionRule(new List<string> { "lenient", "capped", "regional" }),
                new HeadcountOverrideLimitRangeRule(120, 3600),
                new HeadcountOverrideCodeLengthRule(80),
            });
    }
}
