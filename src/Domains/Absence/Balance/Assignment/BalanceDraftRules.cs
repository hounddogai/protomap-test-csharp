namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance draft must pass. Check returns its problems, or none.</summary>
public interface IBalanceDraftRule
{
    List<string> Check(BalanceDraft item);
}

public sealed record BalanceDraftCodePrefixRule(string Prefix) : IBalanceDraftRule
{
    public List<string> Check(BalanceDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceDraftRequiredChannelRule(string Message) : IBalanceDraftRule
{
    public List<string> Check(BalanceDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceDraftAllowedLabelRule(List<string> Allowed) : IBalanceDraftRule
{
    public List<string> Check(BalanceDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record BalanceDraftFloorValueRangeRule(double Min, double Max) : IBalanceDraftRule
{
    public List<string> Check(BalanceDraft item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance draft.</summary>
public sealed record BalanceDraftValidator(List<IBalanceDraftRule> Rules)
{
    public List<string> Validate(BalanceDraft item)
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

    public void RequireValid(BalanceDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceDraft> items)
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

/// <summary>Functions of balance draft rules.</summary>
public static class BalanceDraftRules
{
    public static BalanceDraftValidator DefaultBalanceDraftValidator()
    {
        return new BalanceDraftValidator(
            new List<IBalanceDraftRule>
            {
                new BalanceDraftCodePrefixRule("REF"),
                new BalanceDraftRequiredChannelRule("balance draft needs a value"),
                new BalanceDraftAllowedLabelRule(new List<string> { "legacy" }),
                new BalanceDraftFloorValueRangeRule(8.0, 764.6),
            });
    }
}
