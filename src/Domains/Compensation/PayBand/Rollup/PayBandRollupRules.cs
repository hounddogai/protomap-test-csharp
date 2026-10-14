namespace Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>A rule a pay band rollup must pass. Check returns its problems, or none.</summary>
public interface IPayBandRollupRule
{
    List<string> Check(PayBandRollup item);
}

public sealed record PayBandRollupExpiresAtRangeRule(int Min, int Max) : IPayBandRollupRule
{
    public List<string> Check(PayBandRollup item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandRollupCodeLengthRule(int MaxLength) : IPayBandRollupRule
{
    public List<string> Check(PayBandRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRollupPinnedGuardRule(double Limit) : IPayBandRollupRule
{
    public List<string> Check(PayBandRollup item)
    {
        if (item.Pinned && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRollupFteRangeRule(double Min, double Max) : IPayBandRollupRule
{
    public List<string> Check(PayBandRollup item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRollupBudgetRangeRule(double Min, double Max) : IPayBandRollupRule
{
    public List<string> Check(PayBandRollup item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band rollup.</summary>
public sealed record PayBandRollupValidator(List<IPayBandRollupRule> Rules)
{
    public List<string> Validate(PayBandRollup item)
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

    public void RequireValid(PayBandRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandRollup> items)
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

/// <summary>Functions of pay band rollup rules.</summary>
public static class PayBandRollupRules
{
    public static PayBandRollupValidator DefaultPayBandRollupValidator()
    {
        return new PayBandRollupValidator(
            new List<IPayBandRollupRule>
            {
                new PayBandRollupExpiresAtRangeRule(32, 32),
                new PayBandRollupCodeLengthRule(120),
                new PayBandRollupPinnedGuardRule(3.9),
                new PayBandRollupFteRangeRule(7.7, 871.6),
                new PayBandRollupBudgetRangeRule(9.1, 4472.7),
            });
    }
}
