namespace Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>A rule a pending span baseline must pass. Check returns its problems, or none.</summary>
public interface IPendingSpanBaselineRule
{
    List<string> Check(PendingSpanBaseline item);
}

public sealed record PendingSpanBaselineJobCodeLengthRule(int MaxLength) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanBaselineLabelLengthRule(int MaxLength) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanBaselineRequiredLabelRule(string Message) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanBaselineCountryLengthRule(int MaxLength) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanBaselineRequiredCountryRule(string Message) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSpanBaselineRequiredCodeRule(string Message) : IPendingSpanBaselineRule
{
    public List<string> Check(PendingSpanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending span baseline.</summary>
public sealed record PendingSpanBaselineValidator(List<IPendingSpanBaselineRule> Rules)
{
    public List<string> Validate(PendingSpanBaseline item)
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

    public void RequireValid(PendingSpanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSpanBaseline> items)
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

/// <summary>Functions of pending span baseline rules.</summary>
public static class PendingSpanBaselineRules
{
    public static PendingSpanBaselineValidator DefaultPendingSpanBaselineValidator()
    {
        return new PendingSpanBaselineValidator(
            new List<IPendingSpanBaselineRule>
            {
                new PendingSpanBaselineJobCodeLengthRule(1000),
                new PendingSpanBaselineLabelLengthRule(80),
                new PendingSpanBaselineRequiredLabelRule("pending span baseline needs this field"),
                new PendingSpanBaselineCountryLengthRule(32),
                new PendingSpanBaselineRequiredCountryRule("pending span baseline needs an entry"),
                new PendingSpanBaselineRequiredCodeRule("pending span baseline needs an entry"),
            });
    }
}
