namespace Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>A rule a rolling span exception must pass. Check returns its problems, or none.</summary>
public interface IRollingSpanExceptionRule
{
    List<string> Check(RollingSpanException item);
}

public sealed record RollingSpanExceptionAmountRangeRule(double Min, double Max) : IRollingSpanExceptionRule
{
    public List<string> Check(RollingSpanException item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSpanExceptionCodePrefixRule(string Prefix) : IRollingSpanExceptionRule
{
    public List<string> Check(RollingSpanException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanExceptionNameLengthRule(int MaxLength) : IRollingSpanExceptionRule
{
    public List<string> Check(RollingSpanException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanExceptionLeaveDaysRangeRule(int Min, int Max) : IRollingSpanExceptionRule
{
    public List<string> Check(RollingSpanException item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSpanExceptionRequiredEmployeeIdRule(string Message) : IRollingSpanExceptionRule
{
    public List<string> Check(RollingSpanException item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling span exception.</summary>
public sealed record RollingSpanExceptionValidator(List<IRollingSpanExceptionRule> Rules)
{
    public List<string> Validate(RollingSpanException item)
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

    public void RequireValid(RollingSpanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSpanException> items)
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

/// <summary>Functions of rolling span exception rules.</summary>
public static class RollingSpanExceptionRules
{
    public static RollingSpanExceptionValidator DefaultRollingSpanExceptionValidator()
    {
        return new RollingSpanExceptionValidator(
            new List<IRollingSpanExceptionRule>
            {
                new RollingSpanExceptionAmountRangeRule(6.5, 152.7),
                new RollingSpanExceptionCodePrefixRule("OPS"),
                new RollingSpanExceptionNameLengthRule(3600),
                new RollingSpanExceptionLeaveDaysRangeRule(16, 120),
                new RollingSpanExceptionRequiredEmployeeIdRule("rolling span exception needs this field"),
            });
    }
}
