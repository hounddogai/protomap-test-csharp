namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception digest must pass. Check returns its problems, or none.</summary>
public interface IExceptionDigestRule
{
    List<string> Check(ExceptionDigest item);
}

public sealed record ExceptionDigestRequiredCodeRule(string Message) : IExceptionDigestRule
{
    public List<string> Check(ExceptionDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionDigestCodeLengthRule(int MaxLength) : IExceptionDigestRule
{
    public List<string> Check(ExceptionDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionDigestCodePrefixRule(string Prefix) : IExceptionDigestRule
{
    public List<string> Check(ExceptionDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionDigestRequiredDescriptionRule(string Message) : IExceptionDigestRule
{
    public List<string> Check(ExceptionDigest item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionDigestAllowedNameRule(List<string> Allowed) : IExceptionDigestRule
{
    public List<string> Check(ExceptionDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception digest.</summary>
public sealed record ExceptionDigestValidator(List<IExceptionDigestRule> Rules)
{
    public List<string> Validate(ExceptionDigest item)
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

    public void RequireValid(ExceptionDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionDigest> items)
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

/// <summary>Functions of exception digest rules.</summary>
public static class ExceptionDigestRules
{
    public static ExceptionDigestValidator DefaultExceptionDigestValidator()
    {
        return new ExceptionDigestValidator(
            new List<IExceptionDigestRule>
            {
                new ExceptionDigestRequiredCodeRule("exception digest needs a value"),
                new ExceptionDigestCodeLengthRule(120),
                new ExceptionDigestCodePrefixRule("OPS"),
                new ExceptionDigestRequiredDescriptionRule("exception digest needs a value"),
                new ExceptionDigestAllowedNameRule(new List<string>()),
            });
    }
}
