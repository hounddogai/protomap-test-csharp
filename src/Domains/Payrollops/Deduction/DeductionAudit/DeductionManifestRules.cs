namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction manifest must pass. Check returns its problems, or none.</summary>
public interface IDeductionManifestRule
{
    List<string> Check(DeductionManifest item);
}

public sealed record DeductionManifestNameLengthRule(int MaxLength) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionManifestScoreRangeRule(double Min, double Max) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionManifestCodeLengthRule(int MaxLength) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionManifestRequiredSourceRule(string Message) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionManifestCodePrefixRule(string Prefix) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionManifestAllowedNameRule(List<string> Allowed) : IDeductionManifestRule
{
    public List<string> Check(DeductionManifest item)
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

/// <summary>Runs every rule of a deduction manifest.</summary>
public sealed record DeductionManifestValidator(List<IDeductionManifestRule> Rules)
{
    public List<string> Validate(DeductionManifest item)
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

    public void RequireValid(DeductionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionManifest> items)
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

/// <summary>Functions of deduction manifest rules.</summary>
public static class DeductionManifestRules
{
    public static DeductionManifestValidator DefaultDeductionManifestValidator()
    {
        return new DeductionManifestValidator(
            new List<IDeductionManifestRule>
            {
                new DeductionManifestNameLengthRule(64),
                new DeductionManifestScoreRangeRule(9.5, 2642.1),
                new DeductionManifestCodeLengthRule(255),
                new DeductionManifestRequiredSourceRule("deduction manifest needs a value"),
                new DeductionManifestCodePrefixRule("Q"),
                new DeductionManifestAllowedNameRule(new List<string> { "fallback", "rolling" }),
            });
    }
}
