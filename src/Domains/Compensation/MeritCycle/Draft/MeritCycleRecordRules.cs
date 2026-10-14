namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle record must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleRecordRule
{
    List<string> Check(MeritCycleRecord item);
}

public sealed record MeritCycleRecordRequiredChannelRule(string Message) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRecordCodeLengthRule(int MaxLength) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRecordNameLengthRule(int MaxLength) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRecordRequiredCostCenterRule(string Message) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRecordAllowedCodeRule(List<string> Allowed) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleRecordThresholdRangeRule(double Min, double Max) : IMeritCycleRecordRule
{
    public List<string> Check(MeritCycleRecord item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle record.</summary>
public sealed record MeritCycleRecordValidator(List<IMeritCycleRecordRule> Rules)
{
    public List<string> Validate(MeritCycleRecord item)
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

    public void RequireValid(MeritCycleRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleRecord> items)
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

/// <summary>Functions of merit cycle record rules.</summary>
public static class MeritCycleRecordRules
{
    public static MeritCycleRecordValidator DefaultMeritCycleRecordValidator()
    {
        return new MeritCycleRecordValidator(
            new List<IMeritCycleRecordRule>
            {
                new MeritCycleRecordRequiredChannelRule("merit cycle record needs this field"),
                new MeritCycleRecordCodeLengthRule(86400),
                new MeritCycleRecordNameLengthRule(80),
                new MeritCycleRecordRequiredCostCenterRule("merit cycle record needs an entry"),
                new MeritCycleRecordAllowedCodeRule(new List<string> { "strict", "weighted" }),
                new MeritCycleRecordThresholdRangeRule(4.9, 3132.5),
            });
    }
}
