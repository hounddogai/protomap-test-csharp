namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient tax form estimate must pass. Check returns its problems, or none.</summary>
public interface ILenientTaxFormEstimateRule
{
    List<string> Check(LenientTaxFormEstimate item);
}

public sealed record LenientTaxFormEstimateCodePrefixRule(string Prefix) : ILenientTaxFormEstimateRule
{
    public List<string> Check(LenientTaxFormEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTaxFormEstimateArchivedGuardRule(int Limit) : ILenientTaxFormEstimateRule
{
    public List<string> Check(LenientTaxFormEstimate item)
    {
        if (item.Archived && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LenientTaxFormEstimateAllowedChannelRule(List<string> Allowed) : ILenientTaxFormEstimateRule
{
    public List<string> Check(LenientTaxFormEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientTaxFormEstimateRatioRangeRule(double Min, double Max) : ILenientTaxFormEstimateRule
{
    public List<string> Check(LenientTaxFormEstimate item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientTaxFormEstimateLabelLengthRule(int MaxLength) : ILenientTaxFormEstimateRule
{
    public List<string> Check(LenientTaxFormEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient tax form estimate.</summary>
public sealed record LenientTaxFormEstimateValidator(List<ILenientTaxFormEstimateRule> Rules)
{
    public List<string> Validate(LenientTaxFormEstimate item)
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

    public void RequireValid(LenientTaxFormEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTaxFormEstimate> items)
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

/// <summary>Functions of lenient tax form estimate rules.</summary>
public static class LenientTaxFormEstimateRules
{
    public static LenientTaxFormEstimateValidator DefaultLenientTaxFormEstimateValidator()
    {
        return new LenientTaxFormEstimateValidator(
            new List<ILenientTaxFormEstimateRule>
            {
                new LenientTaxFormEstimateCodePrefixRule("REF"),
                new LenientTaxFormEstimateArchivedGuardRule(86400),
                new LenientTaxFormEstimateAllowedChannelRule(new List<string> { "legacy", "default", "seasonal" }),
                new LenientTaxFormEstimateRatioRangeRule(6.5, 2825.3),
                new LenientTaxFormEstimateLabelLengthRule(64),
            });
    }
}
