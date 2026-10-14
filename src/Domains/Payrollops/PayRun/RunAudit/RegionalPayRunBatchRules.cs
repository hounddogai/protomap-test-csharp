namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional pay run batch must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayRunBatchRule
{
    List<string> Check(RegionalPayRunBatch item);
}

public sealed record RegionalPayRunBatchAllowedCountryRule(List<string> Allowed) : IRegionalPayRunBatchRule
{
    public List<string> Check(RegionalPayRunBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunBatchRequiredSkillRule(string Message) : IRegionalPayRunBatchRule
{
    public List<string> Check(RegionalPayRunBatch item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunBatchCodePrefixRule(string Prefix) : IRegionalPayRunBatchRule
{
    public List<string> Check(RegionalPayRunBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunBatchCodeLengthRule(int MaxLength) : IRegionalPayRunBatchRule
{
    public List<string> Check(RegionalPayRunBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pay run batch.</summary>
public sealed record RegionalPayRunBatchValidator(List<IRegionalPayRunBatchRule> Rules)
{
    public List<string> Validate(RegionalPayRunBatch item)
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

    public void RequireValid(RegionalPayRunBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayRunBatch> items)
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

/// <summary>Functions of regional pay run batch rules.</summary>
public static class RegionalPayRunBatchRules
{
    public static RegionalPayRunBatchValidator DefaultRegionalPayRunBatchValidator()
    {
        return new RegionalPayRunBatchValidator(
            new List<IRegionalPayRunBatchRule>
            {
                new RegionalPayRunBatchAllowedCountryRule(new List<string> { "legacy", "legacy" }),
                new RegionalPayRunBatchRequiredSkillRule("regional pay run batch needs a value"),
                new RegionalPayRunBatchCodePrefixRule("OPS"),
                new RegionalPayRunBatchCodeLengthRule(3600),
            });
    }
}
