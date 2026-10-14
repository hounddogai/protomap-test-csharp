namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model variance must pass. Check returns its problems, or none.</summary>
public interface ICostModelVarianceRule
{
    List<string> Check(CostModelVariance item);
}

public sealed record CostModelVarianceRequiredCodeRule(string Message) : ICostModelVarianceRule
{
    public List<string> Check(CostModelVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelVarianceAllowedCodeRule(List<string> Allowed) : ICostModelVarianceRule
{
    public List<string> Check(CostModelVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record CostModelVarianceCodePrefixRule(string Prefix) : ICostModelVarianceRule
{
    public List<string> Check(CostModelVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelVarianceUnitLengthRule(int MaxLength) : ICostModelVarianceRule
{
    public List<string> Check(CostModelVariance item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model variance.</summary>
public sealed record CostModelVarianceValidator(List<ICostModelVarianceRule> Rules)
{
    public List<string> Validate(CostModelVariance item)
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

    public void RequireValid(CostModelVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelVariance> items)
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

/// <summary>Functions of cost model variance rules.</summary>
public static class CostModelVarianceRules
{
    public static CostModelVarianceValidator DefaultCostModelVarianceValidator()
    {
        return new CostModelVarianceValidator(
            new List<ICostModelVarianceRule>
            {
                new CostModelVarianceRequiredCodeRule("cost model variance needs a value"),
                new CostModelVarianceAllowedCodeRule(new List<string> { "weighted", "scheduled" }),
                new CostModelVarianceCodePrefixRule("X"),
                new CostModelVarianceUnitLengthRule(64),
            });
    }
}
