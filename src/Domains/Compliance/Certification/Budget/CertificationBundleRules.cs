namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification bundle must pass. Check returns its problems, or none.</summary>
public interface ICertificationBundleRule
{
    List<string> Check(CertificationBundle item);
}

public sealed record CertificationBundleRequiredCodeRule(string Message) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationBundleAllowedLabelRule(List<string> Allowed) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CertificationBundleExpiresAtRangeRule(int Min, int Max) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
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

public sealed record CertificationBundleWindowDaysRangeRule(int Min, int Max) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationBundleLabelLengthRule(int MaxLength) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationBundleBatchSizeRangeRule(int Min, int Max) : ICertificationBundleRule
{
    public List<string> Check(CertificationBundle item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification bundle.</summary>
public sealed record CertificationBundleValidator(List<ICertificationBundleRule> Rules)
{
    public List<string> Validate(CertificationBundle item)
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

    public void RequireValid(CertificationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationBundle> items)
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

/// <summary>Functions of certification bundle rules.</summary>
public static class CertificationBundleRules
{
    public static CertificationBundleValidator DefaultCertificationBundleValidator()
    {
        return new CertificationBundleValidator(
            new List<ICertificationBundleRule>
            {
                new CertificationBundleRequiredCodeRule("certification bundle needs a value"),
                new CertificationBundleAllowedLabelRule(new List<string>()),
                new CertificationBundleExpiresAtRangeRule(16, 86400),
                new CertificationBundleWindowDaysRangeRule(16, 64),
                new CertificationBundleLabelLengthRule(16),
                new CertificationBundleBatchSizeRangeRule(120, 3600),
            });
    }
}
