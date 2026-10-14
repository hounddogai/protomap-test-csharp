namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient position estimate must pass. Check returns its problems, or none.</summary>
public interface ILenientPositionEstimateRule
{
    List<string> Check(LenientPositionEstimate item);
}

public sealed record LenientPositionEstimateAllowedUnitRule(List<string> Allowed) : ILenientPositionEstimateRule
{
    public List<string> Check(LenientPositionEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionEstimateSequenceRangeRule(int Min, int Max) : ILenientPositionEstimateRule
{
    public List<string> Check(LenientPositionEstimate item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionEstimateRateRangeRule(double Min, double Max) : ILenientPositionEstimateRule
{
    public List<string> Check(LenientPositionEstimate item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient position estimate.</summary>
public sealed record LenientPositionEstimateValidator(List<ILenientPositionEstimateRule> Rules)
{
    public List<string> Validate(LenientPositionEstimate item)
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

    public void RequireValid(LenientPositionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPositionEstimate> items)
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

/// <summary>Functions of lenient position estimate rules.</summary>
public static class LenientPositionEstimateRules
{
    public static LenientPositionEstimateValidator DefaultLenientPositionEstimateValidator()
    {
        return new LenientPositionEstimateValidator(
            new List<ILenientPositionEstimateRule>
            {
                new LenientPositionEstimateAllowedUnitRule(new List<string> { "primary" }),
                new LenientPositionEstimateSequenceRangeRule(120, 255),
                new LenientPositionEstimateRateRangeRule(2.0, 738.3),
            });
    }
}
