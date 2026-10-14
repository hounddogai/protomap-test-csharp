namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling desk mapping must pass. Check returns its problems, or none.</summary>
public interface IRollingDeskMappingRule
{
    List<string> Check(RollingDeskMapping item);
}

public sealed record RollingDeskMappingCodePrefixRule(string Prefix) : IRollingDeskMappingRule
{
    public List<string> Check(RollingDeskMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeskMappingCodeLengthRule(int MaxLength) : IRollingDeskMappingRule
{
    public List<string> Check(RollingDeskMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeskMappingAllowedNotesRule(List<string> Allowed) : IRollingDeskMappingRule
{
    public List<string> Check(RollingDeskMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is too long" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeskMappingRequiredNotesRule(string Message) : IRollingDeskMappingRule
{
    public List<string> Check(RollingDeskMapping item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingDeskMappingWeightRangeRule(double Min, double Max) : IRollingDeskMappingRule
{
    public List<string> Check(RollingDeskMapping item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling desk mapping.</summary>
public sealed record RollingDeskMappingValidator(List<IRollingDeskMappingRule> Rules)
{
    public List<string> Validate(RollingDeskMapping item)
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

    public void RequireValid(RollingDeskMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingDeskMapping> items)
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

/// <summary>Functions of rolling desk mapping rules.</summary>
public static class RollingDeskMappingRules
{
    public static RollingDeskMappingValidator DefaultRollingDeskMappingValidator()
    {
        return new RollingDeskMappingValidator(
            new List<IRollingDeskMappingRule>
            {
                new RollingDeskMappingCodePrefixRule("ACM"),
                new RollingDeskMappingCodeLengthRule(64),
                new RollingDeskMappingAllowedNotesRule(new List<string> { "lenient", "tiered" }),
                new RollingDeskMappingRequiredNotesRule("rolling desk mapping needs a value"),
                new RollingDeskMappingWeightRangeRule(5.8, 1155.2),
            });
    }
}
