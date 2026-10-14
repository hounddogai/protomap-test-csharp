namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg summary must pass. Check returns its problems, or none.</summary>
public interface IReorgSummaryRule
{
    List<string> Check(ReorgSummary item);
}

public sealed record ReorgSummaryPhoneLengthRule(int MaxLength) : IReorgSummaryRule
{
    public List<string> Check(ReorgSummary item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSummaryRequiredCodeRule(string Message) : IReorgSummaryRule
{
    public List<string> Check(ReorgSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgSummaryAllowedLabelRule(List<string> Allowed) : IReorgSummaryRule
{
    public List<string> Check(ReorgSummary item)
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

public sealed record ReorgSummaryRateRangeRule(double Min, double Max) : IReorgSummaryRule
{
    public List<string> Check(ReorgSummary item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a reorg summary.</summary>
public sealed record ReorgSummaryValidator(List<IReorgSummaryRule> Rules)
{
    public List<string> Validate(ReorgSummary item)
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

    public void RequireValid(ReorgSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgSummary> items)
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

/// <summary>Functions of reorg summary rules.</summary>
public static class ReorgSummaryRules
{
    public static ReorgSummaryValidator DefaultReorgSummaryValidator()
    {
        return new ReorgSummaryValidator(
            new List<IReorgSummaryRule>
            {
                new ReorgSummaryPhoneLengthRule(86400),
                new ReorgSummaryRequiredCodeRule("reorg summary needs an entry"),
                new ReorgSummaryAllowedLabelRule(new List<string> { "weighted", "tiered" }),
                new ReorgSummaryRateRangeRule(9.9, 1956.2),
            });
    }
}
