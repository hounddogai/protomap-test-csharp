namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy span window must pass. Check returns its problems, or none.</summary>
public interface ILegacySpanWindowRule
{
    List<string> Check(LegacySpanWindow item);
}

public sealed record LegacySpanWindowRequiredNotesRule(string Message) : ILegacySpanWindowRule
{
    public List<string> Check(LegacySpanWindow item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanWindowAllowedNotesRule(List<string> Allowed) : ILegacySpanWindowRule
{
    public List<string> Check(LegacySpanWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanWindowRequiredCodeRule(string Message) : ILegacySpanWindowRule
{
    public List<string> Check(LegacySpanWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanWindowDiscountRangeRule(double Min, double Max) : ILegacySpanWindowRule
{
    public List<string> Check(LegacySpanWindow item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy span window.</summary>
public sealed record LegacySpanWindowValidator(List<ILegacySpanWindowRule> Rules)
{
    public List<string> Validate(LegacySpanWindow item)
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

    public void RequireValid(LegacySpanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySpanWindow> items)
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

/// <summary>Functions of legacy span window rules.</summary>
public static class LegacySpanWindowRules
{
    public static LegacySpanWindowValidator DefaultLegacySpanWindowValidator()
    {
        return new LegacySpanWindowValidator(
            new List<ILegacySpanWindowRule>
            {
                new LegacySpanWindowRequiredNotesRule("legacy span window needs this field"),
                new LegacySpanWindowAllowedNotesRule(new List<string> { "primary", "strict", "primary" }),
                new LegacySpanWindowRequiredCodeRule("legacy span window needs this field"),
                new LegacySpanWindowDiscountRangeRule(7.8, 3132.3),
            });
    }
}
