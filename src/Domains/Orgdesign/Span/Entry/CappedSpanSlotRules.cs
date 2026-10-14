namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped span slot must pass. Check returns its problems, or none.</summary>
public interface ICappedSpanSlotRule
{
    List<string> Check(CappedSpanSlot item);
}

public sealed record CappedSpanSlotRequiredCodeRule(string Message) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanSlotCodePrefixRule(string Prefix) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanSlotPhoneLengthRule(int MaxLength) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanSlotRequiredLabelRule(string Message) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanSlotScoreRangeRule(double Min, double Max) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanSlotAllowedUnitRule(List<string> Allowed) : ICappedSpanSlotRule
{
    public List<string> Check(CappedSpanSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped span slot.</summary>
public sealed record CappedSpanSlotValidator(List<ICappedSpanSlotRule> Rules)
{
    public List<string> Validate(CappedSpanSlot item)
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

    public void RequireValid(CappedSpanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSpanSlot> items)
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

/// <summary>Functions of capped span slot rules.</summary>
public static class CappedSpanSlotRules
{
    public static CappedSpanSlotValidator DefaultCappedSpanSlotValidator()
    {
        return new CappedSpanSlotValidator(
            new List<ICappedSpanSlotRule>
            {
                new CappedSpanSlotRequiredCodeRule("capped span slot needs a value"),
                new CappedSpanSlotCodePrefixRule("REF"),
                new CappedSpanSlotPhoneLengthRule(64),
                new CappedSpanSlotRequiredLabelRule("capped span slot needs a value"),
                new CappedSpanSlotScoreRangeRule(6.1, 419.7),
                new CappedSpanSlotAllowedUnitRule(new List<string> { "strict", "capped" }),
            });
    }
}
