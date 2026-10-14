namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global merit cycle rollup must pass. Check returns its problems, or none.</summary>
public interface IGlobalMeritCycleRollupRule
{
    List<string> Check(GlobalMeritCycleRollup item);
}

public sealed record GlobalMeritCycleRollupAllowedPhoneRule(List<string> Allowed) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMeritCycleRollupQuantityRangeRule(int Min, int Max) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMeritCycleRollupNameLengthRule(int MaxLength) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMeritCycleRollupRequiredNameRule(string Message) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalMeritCycleRollupRequiredCodeRule(string Message) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalMeritCycleRollupCodePrefixRule(string Prefix) : IGlobalMeritCycleRollupRule
{
    public List<string> Check(GlobalMeritCycleRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global merit cycle rollup.</summary>
public sealed record GlobalMeritCycleRollupValidator(List<IGlobalMeritCycleRollupRule> Rules)
{
    public List<string> Validate(GlobalMeritCycleRollup item)
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

    public void RequireValid(GlobalMeritCycleRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalMeritCycleRollup> items)
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

/// <summary>Functions of global merit cycle rollup rules.</summary>
public static class GlobalMeritCycleRollupRules
{
    public static GlobalMeritCycleRollupValidator DefaultGlobalMeritCycleRollupValidator()
    {
        return new GlobalMeritCycleRollupValidator(
            new List<IGlobalMeritCycleRollupRule>
            {
                new GlobalMeritCycleRollupAllowedPhoneRule(new List<string> { "rolling", "default", "regional" }),
                new GlobalMeritCycleRollupQuantityRangeRule(3600, 86400),
                new GlobalMeritCycleRollupNameLengthRule(120),
                new GlobalMeritCycleRollupRequiredNameRule("global merit cycle rollup needs an entry"),
                new GlobalMeritCycleRollupRequiredCodeRule("global merit cycle rollup needs a value"),
                new GlobalMeritCycleRollupCodePrefixRule("Q"),
            });
    }
}
