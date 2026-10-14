namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>A rule a legacy span adjustment must pass. Check returns its problems, or none.</summary>
public interface ILegacySpanAdjustmentRule
{
    List<string> Check(LegacySpanAdjustment item);
}

public sealed record LegacySpanAdjustmentNameLengthRule(int MaxLength) : ILegacySpanAdjustmentRule
{
    public List<string> Check(LegacySpanAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanAdjustmentRequiredCostCenterRule(string Message) : ILegacySpanAdjustmentRule
{
    public List<string> Check(LegacySpanAdjustment item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacySpanAdjustmentSequenceRangeRule(int Min, int Max) : ILegacySpanAdjustmentRule
{
    public List<string> Check(LegacySpanAdjustment item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy span adjustment.</summary>
public sealed record LegacySpanAdjustmentValidator(List<ILegacySpanAdjustmentRule> Rules)
{
    public List<string> Validate(LegacySpanAdjustment item)
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

    public void RequireValid(LegacySpanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySpanAdjustment> items)
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

/// <summary>Functions of legacy span adjustment rules.</summary>
public static class LegacySpanAdjustmentRules
{
    public static LegacySpanAdjustmentValidator DefaultLegacySpanAdjustmentValidator()
    {
        return new LegacySpanAdjustmentValidator(
            new List<ILegacySpanAdjustmentRule>
            {
                new LegacySpanAdjustmentNameLengthRule(80),
                new LegacySpanAdjustmentRequiredCostCenterRule("legacy span adjustment needs a value"),
                new LegacySpanAdjustmentSequenceRangeRule(80, 3600),
            });
    }
}
