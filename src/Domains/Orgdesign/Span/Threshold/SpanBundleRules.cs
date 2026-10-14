namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span bundle must pass. Check returns its problems, or none.</summary>
public interface ISpanBundleRule
{
    List<string> Check(SpanBundle item);
}

public sealed record SpanBundlePhoneLengthRule(int MaxLength) : ISpanBundleRule
{
    public List<string> Check(SpanBundle item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBundleRequiredCodeRule(string Message) : ISpanBundleRule
{
    public List<string> Check(SpanBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanBundleAllowedNameRule(List<string> Allowed) : ISpanBundleRule
{
    public List<string> Check(SpanBundle item)
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

public sealed record SpanBundleCodeLengthRule(int MaxLength) : ISpanBundleRule
{
    public List<string> Check(SpanBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBundleRequiredNameRule(string Message) : ISpanBundleRule
{
    public List<string> Check(SpanBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span bundle.</summary>
public sealed record SpanBundleValidator(List<ISpanBundleRule> Rules)
{
    public List<string> Validate(SpanBundle item)
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

    public void RequireValid(SpanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanBundle> items)
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

/// <summary>Functions of span bundle rules.</summary>
public static class SpanBundleRules
{
    public static SpanBundleValidator DefaultSpanBundleValidator()
    {
        return new SpanBundleValidator(
            new List<ISpanBundleRule>
            {
                new SpanBundlePhoneLengthRule(86400),
                new SpanBundleRequiredCodeRule("span bundle needs an entry"),
                new SpanBundleAllowedNameRule(new List<string>()),
                new SpanBundleCodeLengthRule(255),
                new SpanBundleRequiredNameRule("span bundle needs an entry"),
            });
    }
}
