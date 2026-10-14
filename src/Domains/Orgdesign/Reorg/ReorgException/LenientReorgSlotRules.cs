namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>A rule a lenient reorg slot must pass. Check returns its problems, or none.</summary>
public interface ILenientReorgSlotRule
{
    List<string> Check(LenientReorgSlot item);
}

public sealed record LenientReorgSlotCodeLengthRule(int MaxLength) : ILenientReorgSlotRule
{
    public List<string> Check(LenientReorgSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReorgSlotRequiredCodeRule(string Message) : ILenientReorgSlotRule
{
    public List<string> Check(LenientReorgSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientReorgSlotAttemptsRangeRule(int Min, int Max) : ILenientReorgSlotRule
{
    public List<string> Check(LenientReorgSlot item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientReorgSlotCodePrefixRule(string Prefix) : ILenientReorgSlotRule
{
    public List<string> Check(LenientReorgSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReorgSlotSequenceRangeRule(int Min, int Max) : ILenientReorgSlotRule
{
    public List<string> Check(LenientReorgSlot item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient reorg slot.</summary>
public sealed record LenientReorgSlotValidator(List<ILenientReorgSlotRule> Rules)
{
    public List<string> Validate(LenientReorgSlot item)
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

    public void RequireValid(LenientReorgSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientReorgSlot> items)
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

/// <summary>Functions of lenient reorg slot rules.</summary>
public static class LenientReorgSlotRules
{
    public static LenientReorgSlotValidator DefaultLenientReorgSlotValidator()
    {
        return new LenientReorgSlotValidator(
            new List<ILenientReorgSlotRule>
            {
                new LenientReorgSlotCodeLengthRule(80),
                new LenientReorgSlotRequiredCodeRule("lenient reorg slot needs an entry"),
                new LenientReorgSlotAttemptsRangeRule(80, 120),
                new LenientReorgSlotCodePrefixRule("ACM"),
                new LenientReorgSlotSequenceRangeRule(16, 86400),
            });
    }
}
