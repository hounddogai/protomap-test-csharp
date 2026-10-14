namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk preference must pass. Check returns its problems, or none.</summary>
public interface IDeskPreferenceRule
{
    List<string> Check(DeskPreference item);
}

public sealed record DeskPreferenceRequiredCodeRule(string Message) : IDeskPreferenceRule
{
    public List<string> Check(DeskPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskPreferenceCodeLengthRule(int MaxLength) : IDeskPreferenceRule
{
    public List<string> Check(DeskPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskPreferenceAllowedBadgeRule(List<string> Allowed) : IDeskPreferenceRule
{
    public List<string> Check(DeskPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DeskPreferenceVersionRangeRule(int Min, int Max) : IDeskPreferenceRule
{
    public List<string> Check(DeskPreference item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk preference.</summary>
public sealed record DeskPreferenceValidator(List<IDeskPreferenceRule> Rules)
{
    public List<string> Validate(DeskPreference item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(DeskPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskPreference> items)
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

/// <summary>Functions of desk preference rules.</summary>
public static class DeskPreferenceRules
{
    public static DeskPreferenceValidator DefaultDeskPreferenceValidator()
    {
        return new DeskPreferenceValidator(
            new List<IDeskPreferenceRule>
            {
                new DeskPreferenceRequiredCodeRule("desk preference needs a value"),
                new DeskPreferenceCodeLengthRule(80),
                new DeskPreferenceAllowedBadgeRule(new List<string> { "batched" }),
                new DeskPreferenceVersionRangeRule(16, 255),
            });
    }
}
