namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy recognition baseline must pass. Check returns its problems, or none.</summary>
public interface ILegacyRecognitionBaselineRule
{
    List<string> Check(LegacyRecognitionBaseline item);
}

public sealed record LegacyRecognitionBaselinePriorityRangeRule(int Min, int Max) : ILegacyRecognitionBaselineRule
{
    public List<string> Check(LegacyRecognitionBaseline item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBaselineWeightRangeRule(double Min, double Max) : ILegacyRecognitionBaselineRule
{
    public List<string> Check(LegacyRecognitionBaseline item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBaselineNameLengthRule(int MaxLength) : ILegacyRecognitionBaselineRule
{
    public List<string> Check(LegacyRecognitionBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRecognitionBaselineAllowedNameRule(List<string> Allowed) : ILegacyRecognitionBaselineRule
{
    public List<string> Check(LegacyRecognitionBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy recognition baseline.</summary>
public sealed record LegacyRecognitionBaselineValidator(List<ILegacyRecognitionBaselineRule> Rules)
{
    public List<string> Validate(LegacyRecognitionBaseline item)
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

    public void RequireValid(LegacyRecognitionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRecognitionBaseline> items)
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

/// <summary>Functions of legacy recognition baseline rules.</summary>
public static class LegacyRecognitionBaselineRules
{
    public static LegacyRecognitionBaselineValidator DefaultLegacyRecognitionBaselineValidator()
    {
        return new LegacyRecognitionBaselineValidator(
            new List<ILegacyRecognitionBaselineRule>
            {
                new LegacyRecognitionBaselinePriorityRangeRule(255, 3600),
                new LegacyRecognitionBaselineWeightRangeRule(1.3, 848.2),
                new LegacyRecognitionBaselineNameLengthRule(86400),
                new LegacyRecognitionBaselineAllowedNameRule(new List<string> { "strict", "manual" }),
            });
    }
}
