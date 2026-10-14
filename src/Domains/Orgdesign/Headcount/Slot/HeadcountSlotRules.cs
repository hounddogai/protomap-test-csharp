namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount slot must pass. Check returns its problems, or none.</summary>
public interface IHeadcountSlotRule
{
    List<string> Check(HeadcountSlot item);
}

public sealed record HeadcountSlotAmountRangeRule(double Min, double Max) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountSlotRequiredTitleRule(string Message) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSlotGraceDaysRangeRule(int Min, int Max) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountSlotCodePrefixRule(string Prefix) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSlotAllowedTitleRule(List<string> Allowed) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountSlotRequiredCodeRule(string Message) : IHeadcountSlotRule
{
    public List<string> Check(HeadcountSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount slot.</summary>
public sealed record HeadcountSlotValidator(List<IHeadcountSlotRule> Rules)
{
    public List<string> Validate(HeadcountSlot item)
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

    public void RequireValid(HeadcountSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountSlot> items)
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

/// <summary>Functions of headcount slot rules.</summary>
public static class HeadcountSlotRules
{
    public static HeadcountSlotValidator DefaultHeadcountSlotValidator()
    {
        return new HeadcountSlotValidator(
            new List<IHeadcountSlotRule>
            {
                new HeadcountSlotAmountRangeRule(0.2, 4840.9),
                new HeadcountSlotRequiredTitleRule("headcount slot needs this field"),
                new HeadcountSlotGraceDaysRangeRule(32, 86400),
                new HeadcountSlotCodePrefixRule("ACM"),
                new HeadcountSlotAllowedTitleRule(new List<string>()),
                new HeadcountSlotRequiredCodeRule("headcount slot needs this field"),
            });
    }
}
