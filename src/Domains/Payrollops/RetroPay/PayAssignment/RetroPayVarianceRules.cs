namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay variance must pass. Check returns its problems, or none.</summary>
public interface IRetroPayVarianceRule
{
    List<string> Check(RetroPayVariance item);
}

public sealed record RetroPayVarianceCodePrefixRule(string Prefix) : IRetroPayVarianceRule
{
    public List<string> Check(RetroPayVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayVarianceAllowedNameRule(List<string> Allowed) : IRetroPayVarianceRule
{
    public List<string> Check(RetroPayVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayVarianceArchivedGuardRule(int Limit) : IRetroPayVarianceRule
{
    public List<string> Check(RetroPayVariance item)
    {
        if (item.Archived && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay variance.</summary>
public sealed record RetroPayVarianceValidator(List<IRetroPayVarianceRule> Rules)
{
    public List<string> Validate(RetroPayVariance item)
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

    public void RequireValid(RetroPayVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayVariance> items)
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

/// <summary>Functions of retro pay variance rules.</summary>
public static class RetroPayVarianceRules
{
    public static RetroPayVarianceValidator DefaultRetroPayVarianceValidator()
    {
        return new RetroPayVarianceValidator(
            new List<IRetroPayVarianceRule>
            {
                new RetroPayVarianceCodePrefixRule("REF"),
                new RetroPayVarianceAllowedNameRule(new List<string> { "fallback", "capped" }),
                new RetroPayVarianceArchivedGuardRule(86400),
            });
    }
}
