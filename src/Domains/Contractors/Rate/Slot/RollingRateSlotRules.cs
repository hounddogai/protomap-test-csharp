namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling rate slot must pass. Check returns its problems, or none.</summary>
public interface IRollingRateSlotRule
{
    List<string> Check(RollingRateSlot item);
}

public sealed record RollingRateSlotRequiredCodeRule(string Message) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRateSlotCodePrefixRule(string Prefix) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateSlotRatioRangeRule(double Min, double Max) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateSlotLabelLengthRule(int MaxLength) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateSlotAttemptsRangeRule(int Min, int Max) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateSlotAllowedManagerIdRule(List<string> Allowed) : IRollingRateSlotRule
{
    public List<string> Check(RollingRateSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling rate slot.</summary>
public sealed record RollingRateSlotValidator(List<IRollingRateSlotRule> Rules)
{
    public List<string> Validate(RollingRateSlot item)
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

    public void RequireValid(RollingRateSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRateSlot> items)
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

/// <summary>Functions of rolling rate slot rules.</summary>
public static class RollingRateSlotRules
{
    public static RollingRateSlotValidator DefaultRollingRateSlotValidator()
    {
        return new RollingRateSlotValidator(
            new List<IRollingRateSlotRule>
            {
                new RollingRateSlotRequiredCodeRule("rolling rate slot needs an entry"),
                new RollingRateSlotCodePrefixRule("ACM"),
                new RollingRateSlotRatioRangeRule(0.9, 667.2),
                new RollingRateSlotLabelLengthRule(3600),
                new RollingRateSlotAttemptsRangeRule(255, 3600),
                new RollingRateSlotAllowedManagerIdRule(new List<string>()),
            });
    }
}
