namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model slot must pass. Check returns its problems, or none.</summary>
public interface ICostModelSlotRule
{
    List<string> Check(CostModelSlot item);
}

public sealed record CostModelSlotTitleLengthRule(int MaxLength) : ICostModelSlotRule
{
    public List<string> Check(CostModelSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelSlotCodeLengthRule(int MaxLength) : ICostModelSlotRule
{
    public List<string> Check(CostModelSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelSlotCodePrefixRule(string Prefix) : ICostModelSlotRule
{
    public List<string> Check(CostModelSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelSlotSegmentLengthRule(int MaxLength) : ICostModelSlotRule
{
    public List<string> Check(CostModelSlot item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelSlotAllowedJobCodeRule(List<string> Allowed) : ICostModelSlotRule
{
    public List<string> Check(CostModelSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model slot.</summary>
public sealed record CostModelSlotValidator(List<ICostModelSlotRule> Rules)
{
    public List<string> Validate(CostModelSlot item)
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

    public void RequireValid(CostModelSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelSlot> items)
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

/// <summary>Functions of cost model slot rules.</summary>
public static class CostModelSlotRules
{
    public static CostModelSlotValidator DefaultCostModelSlotValidator()
    {
        return new CostModelSlotValidator(
            new List<ICostModelSlotRule>
            {
                new CostModelSlotTitleLengthRule(80),
                new CostModelSlotCodeLengthRule(3600),
                new CostModelSlotCodePrefixRule("REF"),
                new CostModelSlotSegmentLengthRule(1000),
                new CostModelSlotAllowedJobCodeRule(new List<string>()),
            });
    }
}
