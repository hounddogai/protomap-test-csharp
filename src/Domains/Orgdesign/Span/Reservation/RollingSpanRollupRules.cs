namespace Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>A rule a rolling span rollup must pass. Check returns its problems, or none.</summary>
public interface IRollingSpanRollupRule
{
    List<string> Check(RollingSpanRollup item);
}

public sealed record RollingSpanRollupExpiresAtRangeRule(int Min, int Max) : IRollingSpanRollupRule
{
    public List<string> Check(RollingSpanRollup item)
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

public sealed record RollingSpanRollupRequiredCodeRule(string Message) : IRollingSpanRollupRule
{
    public List<string> Check(RollingSpanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanRollupCodeLengthRule(int MaxLength) : IRollingSpanRollupRule
{
    public List<string> Check(RollingSpanRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling span rollup.</summary>
public sealed record RollingSpanRollupValidator(List<IRollingSpanRollupRule> Rules)
{
    public List<string> Validate(RollingSpanRollup item)
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

    public void RequireValid(RollingSpanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSpanRollup> items)
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

/// <summary>Functions of rolling span rollup rules.</summary>
public static class RollingSpanRollupRules
{
    public static RollingSpanRollupValidator DefaultRollingSpanRollupValidator()
    {
        return new RollingSpanRollupValidator(
            new List<IRollingSpanRollupRule>
            {
                new RollingSpanRollupExpiresAtRangeRule(64, 80),
                new RollingSpanRollupRequiredCodeRule("rolling span rollup needs an entry"),
                new RollingSpanRollupCodeLengthRule(1000),
            });
    }
}
