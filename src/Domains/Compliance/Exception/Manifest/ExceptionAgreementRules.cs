namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception agreement must pass. Check returns its problems, or none.</summary>
public interface IExceptionAgreementRule
{
    List<string> Check(ExceptionAgreement item);
}

public sealed record ExceptionAgreementTitleLengthRule(int MaxLength) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAgreementDurationMinutesRangeRule(int Min, int Max) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAgreementAllowedCodeRule(List<string> Allowed) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
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

public sealed record ExceptionAgreementRetriesRangeRule(int Min, int Max) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAgreementAttemptsRangeRule(int Min, int Max) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAgreementCodePrefixRule(string Prefix) : IExceptionAgreementRule
{
    public List<string> Check(ExceptionAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception agreement.</summary>
public sealed record ExceptionAgreementValidator(List<IExceptionAgreementRule> Rules)
{
    public List<string> Validate(ExceptionAgreement item)
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

    public void RequireValid(ExceptionAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionAgreement> items)
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

/// <summary>Functions of exception agreement rules.</summary>
public static class ExceptionAgreementRules
{
    public static ExceptionAgreementValidator DefaultExceptionAgreementValidator()
    {
        return new ExceptionAgreementValidator(
            new List<IExceptionAgreementRule>
            {
                new ExceptionAgreementTitleLengthRule(32),
                new ExceptionAgreementDurationMinutesRangeRule(32, 86400),
                new ExceptionAgreementAllowedCodeRule(new List<string> { "rolling", "regional" }),
                new ExceptionAgreementRetriesRangeRule(16, 1000),
                new ExceptionAgreementAttemptsRangeRule(64, 80),
                new ExceptionAgreementCodePrefixRule("REF"),
            });
    }
}
