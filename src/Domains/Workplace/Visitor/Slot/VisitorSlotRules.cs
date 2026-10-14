namespace Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>A rule a visitor slot must pass. Check returns its problems, or none.</summary>
public interface IVisitorSlotRule
{
    List<string> Check(VisitorSlot item);
}

public sealed record VisitorSlotCodePrefixRule(string Prefix) : IVisitorSlotRule
{
    public List<string> Check(VisitorSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorSlotRequiredTimezoneRule(string Message) : IVisitorSlotRule
{
    public List<string> Check(VisitorSlot item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorSlotThresholdRangeRule(double Min, double Max) : IVisitorSlotRule
{
    public List<string> Check(VisitorSlot item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor slot.</summary>
public sealed record VisitorSlotValidator(List<IVisitorSlotRule> Rules)
{
    public List<string> Validate(VisitorSlot item)
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

    public void RequireValid(VisitorSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorSlot> items)
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

/// <summary>Functions of visitor slot rules.</summary>
public static class VisitorSlotRules
{
    public static VisitorSlotValidator DefaultVisitorSlotValidator()
    {
        return new VisitorSlotValidator(
            new List<IVisitorSlotRule>
            {
                new VisitorSlotCodePrefixRule("Q"),
                new VisitorSlotRequiredTimezoneRule("visitor slot needs this field"),
                new VisitorSlotThresholdRangeRule(0.3, 4366.4),
            });
    }
}
