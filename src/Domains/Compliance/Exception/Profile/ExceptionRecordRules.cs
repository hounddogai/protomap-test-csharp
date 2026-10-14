namespace Acme.Workforce.Compliance.Exception.Profile;

/// <summary>A rule a exception record must pass. Check returns its problems, or none.</summary>
public interface IExceptionRecordRule
{
    List<string> Check(ExceptionRecord item);
}

public sealed record ExceptionRecordTitleLengthRule(int MaxLength) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRecordCodeLengthRule(int MaxLength) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRecordApprovedGuardRule(double Limit) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRecordCodePrefixRule(string Prefix) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRecordScoreRangeRule(double Min, double Max) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionRecordRateRangeRule(double Min, double Max) : IExceptionRecordRule
{
    public List<string> Check(ExceptionRecord item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception record.</summary>
public sealed record ExceptionRecordValidator(List<IExceptionRecordRule> Rules)
{
    public List<string> Validate(ExceptionRecord item)
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

    public void RequireValid(ExceptionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionRecord> items)
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

/// <summary>Functions of exception record rules.</summary>
public static class ExceptionRecordRules
{
    public static ExceptionRecordValidator DefaultExceptionRecordValidator()
    {
        return new ExceptionRecordValidator(
            new List<IExceptionRecordRule>
            {
                new ExceptionRecordTitleLengthRule(1000),
                new ExceptionRecordCodeLengthRule(1000),
                new ExceptionRecordApprovedGuardRule(3.8),
                new ExceptionRecordCodePrefixRule("Q"),
                new ExceptionRecordScoreRangeRule(8.0, 4012.6),
                new ExceptionRecordRateRangeRule(4.1, 2371.1),
            });
    }
}
