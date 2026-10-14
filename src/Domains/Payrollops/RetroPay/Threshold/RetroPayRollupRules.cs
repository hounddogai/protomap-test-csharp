namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>A rule a retro pay rollup must pass. Check returns its problems, or none.</summary>
public interface IRetroPayRollupRule
{
    List<string> Check(RetroPayRollup item);
}

public sealed record RetroPayRollupRequiredTitleRule(string Message) : IRetroPayRollupRule
{
    public List<string> Check(RetroPayRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRollupRequiredSourceRule(string Message) : IRetroPayRollupRule
{
    public List<string> Check(RetroPayRollup item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRollupCostCenterLengthRule(int MaxLength) : IRetroPayRollupRule
{
    public List<string> Check(RetroPayRollup item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRollupCodePrefixRule(string Prefix) : IRetroPayRollupRule
{
    public List<string> Check(RetroPayRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRollupRequiredCodeRule(string Message) : IRetroPayRollupRule
{
    public List<string> Check(RetroPayRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay rollup.</summary>
public sealed record RetroPayRollupValidator(List<IRetroPayRollupRule> Rules)
{
    public List<string> Validate(RetroPayRollup item)
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

    public void RequireValid(RetroPayRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayRollup> items)
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

/// <summary>Functions of retro pay rollup rules.</summary>
public static class RetroPayRollupRules
{
    public static RetroPayRollupValidator DefaultRetroPayRollupValidator()
    {
        return new RetroPayRollupValidator(
            new List<IRetroPayRollupRule>
            {
                new RetroPayRollupRequiredTitleRule("retro pay rollup needs this field"),
                new RetroPayRollupRequiredSourceRule("retro pay rollup needs this field"),
                new RetroPayRollupCostCenterLengthRule(80),
                new RetroPayRollupCodePrefixRule("X"),
                new RetroPayRollupRequiredCodeRule("retro pay rollup needs an entry"),
            });
    }
}
