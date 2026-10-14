namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>A rule a tax form manifest must pass. Check returns its problems, or none.</summary>
public interface ITaxFormManifestRule
{
    List<string> Check(TaxFormManifest item);
}

public sealed record TaxFormManifestScoreRangeRule(double Min, double Max) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormManifestCodeLengthRule(int MaxLength) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormManifestCodePrefixRule(string Prefix) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormManifestRequiredTitleRule(string Message) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormManifestRequiredCodeRule(string Message) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormManifestRequiredSummaryRule(string Message) : ITaxFormManifestRule
{
    public List<string> Check(TaxFormManifest item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form manifest.</summary>
public sealed record TaxFormManifestValidator(List<ITaxFormManifestRule> Rules)
{
    public List<string> Validate(TaxFormManifest item)
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

    public void RequireValid(TaxFormManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormManifest> items)
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

/// <summary>Functions of tax form manifest rules.</summary>
public static class TaxFormManifestRules
{
    public static TaxFormManifestValidator DefaultTaxFormManifestValidator()
    {
        return new TaxFormManifestValidator(
            new List<ITaxFormManifestRule>
            {
                new TaxFormManifestScoreRangeRule(5.9, 3927.0),
                new TaxFormManifestCodeLengthRule(86400),
                new TaxFormManifestCodePrefixRule("ACM"),
                new TaxFormManifestRequiredTitleRule("tax form manifest needs this field"),
                new TaxFormManifestRequiredCodeRule("tax form manifest needs this field"),
                new TaxFormManifestRequiredSummaryRule("tax form manifest needs this field"),
            });
    }
}
