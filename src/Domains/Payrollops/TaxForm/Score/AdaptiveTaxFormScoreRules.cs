namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive tax form score must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTaxFormScoreRule
{
    List<string> Check(AdaptiveTaxFormScore item);
}

public sealed record AdaptiveTaxFormScoreAllowedCodeRule(List<string> Allowed) : IAdaptiveTaxFormScoreRule
{
    public List<string> Check(AdaptiveTaxFormScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTaxFormScoreNameLengthRule(int MaxLength) : IAdaptiveTaxFormScoreRule
{
    public List<string> Check(AdaptiveTaxFormScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTaxFormScoreCodeLengthRule(int MaxLength) : IAdaptiveTaxFormScoreRule
{
    public List<string> Check(AdaptiveTaxFormScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive tax form score.</summary>
public sealed record AdaptiveTaxFormScoreValidator(List<IAdaptiveTaxFormScoreRule> Rules)
{
    public List<string> Validate(AdaptiveTaxFormScore item)
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

    public void RequireValid(AdaptiveTaxFormScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTaxFormScore> items)
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

/// <summary>Functions of adaptive tax form score rules.</summary>
public static class AdaptiveTaxFormScoreRules
{
    public static AdaptiveTaxFormScoreValidator DefaultAdaptiveTaxFormScoreValidator()
    {
        return new AdaptiveTaxFormScoreValidator(
            new List<IAdaptiveTaxFormScoreRule>
            {
                new AdaptiveTaxFormScoreAllowedCodeRule(new List<string>()),
                new AdaptiveTaxFormScoreNameLengthRule(64),
                new AdaptiveTaxFormScoreCodeLengthRule(255),
            });
    }
}
