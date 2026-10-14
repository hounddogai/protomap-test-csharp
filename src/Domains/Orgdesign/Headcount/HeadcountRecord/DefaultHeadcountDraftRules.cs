namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>A rule a default headcount draft must pass. Check returns its problems, or none.</summary>
public interface IDefaultHeadcountDraftRule
{
    List<string> Check(DefaultHeadcountDraft item);
}

public sealed record DefaultHeadcountDraftLabelLengthRule(int MaxLength) : IDefaultHeadcountDraftRule
{
    public List<string> Check(DefaultHeadcountDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountDraftCostRangeRule(double Min, double Max) : IDefaultHeadcountDraftRule
{
    public List<string> Check(DefaultHeadcountDraft item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountDraftStartsAtRangeRule(int Min, int Max) : IDefaultHeadcountDraftRule
{
    public List<string> Check(DefaultHeadcountDraft item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default headcount draft.</summary>
public sealed record DefaultHeadcountDraftValidator(List<IDefaultHeadcountDraftRule> Rules)
{
    public List<string> Validate(DefaultHeadcountDraft item)
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

    public void RequireValid(DefaultHeadcountDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHeadcountDraft> items)
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

/// <summary>Functions of default headcount draft rules.</summary>
public static class DefaultHeadcountDraftRules
{
    public static DefaultHeadcountDraftValidator DefaultDefaultHeadcountDraftValidator()
    {
        return new DefaultHeadcountDraftValidator(
            new List<IDefaultHeadcountDraftRule>
            {
                new DefaultHeadcountDraftLabelLengthRule(255),
                new DefaultHeadcountDraftCostRangeRule(3.2, 847.1),
                new DefaultHeadcountDraftStartsAtRangeRule(32, 86400),
            });
    }
}
