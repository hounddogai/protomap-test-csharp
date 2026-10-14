namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pay band draft must pass. Check returns its problems, or none.</summary>
public interface IStrictPayBandDraftRule
{
    List<string> Check(StrictPayBandDraft item);
}

public sealed record StrictPayBandDraftAllowedChannelRule(List<string> Allowed) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandDraftDiscountRangeRule(double Min, double Max) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandDraftRequiredChannelRule(string Message) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandDraftRequiredCodeRule(string Message) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandDraftNameLengthRule(int MaxLength) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandDraftRequiredNameRule(string Message) : IStrictPayBandDraftRule
{
    public List<string> Check(StrictPayBandDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pay band draft.</summary>
public sealed record StrictPayBandDraftValidator(List<IStrictPayBandDraftRule> Rules)
{
    public List<string> Validate(StrictPayBandDraft item)
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

    public void RequireValid(StrictPayBandDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPayBandDraft> items)
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

/// <summary>Functions of strict pay band draft rules.</summary>
public static class StrictPayBandDraftRules
{
    public static StrictPayBandDraftValidator DefaultStrictPayBandDraftValidator()
    {
        return new StrictPayBandDraftValidator(
            new List<IStrictPayBandDraftRule>
            {
                new StrictPayBandDraftAllowedChannelRule(new List<string> { "global", "global", "tiered" }),
                new StrictPayBandDraftDiscountRangeRule(8.0, 3425.6),
                new StrictPayBandDraftRequiredChannelRule("strict pay band draft needs a value"),
                new StrictPayBandDraftRequiredCodeRule("strict pay band draft needs an entry"),
                new StrictPayBandDraftNameLengthRule(16),
                new StrictPayBandDraftRequiredNameRule("strict pay band draft needs an entry"),
            });
    }
}
