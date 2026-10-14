namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor rollup must pass. Check returns its problems, or none.</summary>
public interface IVisitorRollupRule
{
    List<string> Check(VisitorRollup item);
}

public sealed record VisitorRollupUnitLengthRule(int MaxLength) : IVisitorRollupRule
{
    public List<string> Check(VisitorRollup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRollupRequiredUnitRule(string Message) : IVisitorRollupRule
{
    public List<string> Check(VisitorRollup item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorRollupAllowedNotesRule(List<string> Allowed) : IVisitorRollupRule
{
    public List<string> Check(VisitorRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRollupCodePrefixRule(string Prefix) : IVisitorRollupRule
{
    public List<string> Check(VisitorRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRollupCodeLengthRule(int MaxLength) : IVisitorRollupRule
{
    public List<string> Check(VisitorRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor rollup.</summary>
public sealed record VisitorRollupValidator(List<IVisitorRollupRule> Rules)
{
    public List<string> Validate(VisitorRollup item)
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

    public void RequireValid(VisitorRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorRollup> items)
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

/// <summary>Functions of visitor rollup rules.</summary>
public static class VisitorRollupRules
{
    public static VisitorRollupValidator DefaultVisitorRollupValidator()
    {
        return new VisitorRollupValidator(
            new List<IVisitorRollupRule>
            {
                new VisitorRollupUnitLengthRule(86400),
                new VisitorRollupRequiredUnitRule("visitor rollup needs this field"),
                new VisitorRollupAllowedNotesRule(new List<string> { "regional" }),
                new VisitorRollupCodePrefixRule("Q"),
                new VisitorRollupCodeLengthRule(32),
            });
    }
}
