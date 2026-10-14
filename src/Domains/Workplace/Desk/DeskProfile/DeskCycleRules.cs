namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk cycle must pass. Check returns its problems, or none.</summary>
public interface IDeskCycleRule
{
    List<string> Check(DeskCycle item);
}

public sealed record DeskCycleCodeLengthRule(int MaxLength) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCycleCodePrefixRule(string Prefix) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCycleRequiredManagerIdRule(string Message) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskCycleTitleLengthRule(int MaxLength) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskCycleAllowedTimezoneRule(List<string> Allowed) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record DeskCycleManagerIdLengthRule(int MaxLength) : IDeskCycleRule
{
    public List<string> Check(DeskCycle item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk cycle.</summary>
public sealed record DeskCycleValidator(List<IDeskCycleRule> Rules)
{
    public List<string> Validate(DeskCycle item)
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

    public void RequireValid(DeskCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskCycle> items)
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

/// <summary>Functions of desk cycle rules.</summary>
public static class DeskCycleRules
{
    public static DeskCycleValidator DefaultDeskCycleValidator()
    {
        return new DeskCycleValidator(
            new List<IDeskCycleRule>
            {
                new DeskCycleCodeLengthRule(80),
                new DeskCycleCodePrefixRule("OPS"),
                new DeskCycleRequiredManagerIdRule("desk cycle needs this field"),
                new DeskCycleTitleLengthRule(86400),
                new DeskCycleAllowedTimezoneRule(new List<string> { "manual", "manual", "scheduled" }),
                new DeskCycleManagerIdLengthRule(255),
            });
    }
}
