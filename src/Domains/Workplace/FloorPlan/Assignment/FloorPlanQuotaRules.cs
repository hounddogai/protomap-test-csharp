namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan quota must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanQuotaRule
{
    List<string> Check(FloorPlanQuota item);
}

public sealed record FloorPlanQuotaCodePrefixRule(string Prefix) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanQuotaNameLengthRule(int MaxLength) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanQuotaAllowedCodeRule(List<string> Allowed) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanQuotaSequenceRangeRule(int Min, int Max) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanQuotaRequiredNameRule(string Message) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanQuotaGradeLengthRule(int MaxLength) : IFloorPlanQuotaRule
{
    public List<string> Check(FloorPlanQuota item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan quota.</summary>
public sealed record FloorPlanQuotaValidator(List<IFloorPlanQuotaRule> Rules)
{
    public List<string> Validate(FloorPlanQuota item)
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

    public void RequireValid(FloorPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanQuota> items)
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

/// <summary>Functions of floor plan quota rules.</summary>
public static class FloorPlanQuotaRules
{
    public static FloorPlanQuotaValidator DefaultFloorPlanQuotaValidator()
    {
        return new FloorPlanQuotaValidator(
            new List<IFloorPlanQuotaRule>
            {
                new FloorPlanQuotaCodePrefixRule("REF"),
                new FloorPlanQuotaNameLengthRule(16),
                new FloorPlanQuotaAllowedCodeRule(new List<string>()),
                new FloorPlanQuotaSequenceRangeRule(32, 86400),
                new FloorPlanQuotaRequiredNameRule("floor plan quota needs this field"),
                new FloorPlanQuotaGradeLengthRule(1000),
            });
    }
}
