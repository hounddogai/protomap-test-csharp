namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pay band draft must pass. Check returns its problems, or none.</summary>
public interface ITieredPayBandDraftRule
{
    List<string> Check(TieredPayBandDraft item);
}

public sealed record TieredPayBandDraftRequiredLabelRule(string Message) : ITieredPayBandDraftRule
{
    public List<string> Check(TieredPayBandDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandDraftRequiredCodeRule(string Message) : ITieredPayBandDraftRule
{
    public List<string> Check(TieredPayBandDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandDraftHoursRangeRule(double Min, double Max) : ITieredPayBandDraftRule
{
    public List<string> Check(TieredPayBandDraft item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandDraftAllowedLabelRule(List<string> Allowed) : ITieredPayBandDraftRule
{
    public List<string> Check(TieredPayBandDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay band draft.</summary>
public sealed record TieredPayBandDraftValidator(List<ITieredPayBandDraftRule> Rules)
{
    public List<string> Validate(TieredPayBandDraft item)
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

    public void RequireValid(TieredPayBandDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayBandDraft> items)
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

/// <summary>Functions of tiered pay band draft rules.</summary>
public static class TieredPayBandDraftRules
{
    public static TieredPayBandDraftValidator DefaultTieredPayBandDraftValidator()
    {
        return new TieredPayBandDraftValidator(
            new List<ITieredPayBandDraftRule>
            {
                new TieredPayBandDraftRequiredLabelRule("tiered pay band draft needs this field"),
                new TieredPayBandDraftRequiredCodeRule("tiered pay band draft needs an entry"),
                new TieredPayBandDraftHoursRangeRule(8.1, 1259.6),
                new TieredPayBandDraftAllowedLabelRule(new List<string> { "seasonal" }),
            });
    }
}
