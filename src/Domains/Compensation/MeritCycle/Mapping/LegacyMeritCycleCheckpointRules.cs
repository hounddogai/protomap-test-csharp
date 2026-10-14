namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy merit cycle checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILegacyMeritCycleCheckpointRule
{
    List<string> Check(LegacyMeritCycleCheckpoint item);
}

public sealed record LegacyMeritCycleCheckpointDiscountRangeRule(double Min, double Max) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleCheckpointRequiredJobCodeRule(string Message) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleCheckpointRegionLengthRule(int MaxLength) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleCheckpointAllowedRegionRule(List<string> Allowed) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleCheckpointBatchSizeRangeRule(int Min, int Max) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyMeritCycleCheckpointCodeLengthRule(int MaxLength) : ILegacyMeritCycleCheckpointRule
{
    public List<string> Check(LegacyMeritCycleCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy merit cycle checkpoint.</summary>
public sealed record LegacyMeritCycleCheckpointValidator(List<ILegacyMeritCycleCheckpointRule> Rules)
{
    public List<string> Validate(LegacyMeritCycleCheckpoint item)
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

    public void RequireValid(LegacyMeritCycleCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyMeritCycleCheckpoint> items)
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

/// <summary>Functions of legacy merit cycle checkpoint rules.</summary>
public static class LegacyMeritCycleCheckpointRules
{
    public static LegacyMeritCycleCheckpointValidator DefaultLegacyMeritCycleCheckpointValidator()
    {
        return new LegacyMeritCycleCheckpointValidator(
            new List<ILegacyMeritCycleCheckpointRule>
            {
                new LegacyMeritCycleCheckpointDiscountRangeRule(9.9, 1107.3),
                new LegacyMeritCycleCheckpointRequiredJobCodeRule("legacy merit cycle checkpoint needs an entry"),
                new LegacyMeritCycleCheckpointRegionLengthRule(3600),
                new LegacyMeritCycleCheckpointAllowedRegionRule(new List<string> { "default" }),
                new LegacyMeritCycleCheckpointBatchSizeRangeRule(120, 3600),
                new LegacyMeritCycleCheckpointCodeLengthRule(3600),
            });
    }
}
