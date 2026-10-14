namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification window must pass. Check returns its problems, or none.</summary>
public interface ICertificationWindowRule
{
    List<string> Check(CertificationWindow item);
}

public sealed record CertificationWindowLabelLengthRule(int MaxLength) : ICertificationWindowRule
{
    public List<string> Check(CertificationWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationWindowRequiredCodeRule(string Message) : ICertificationWindowRule
{
    public List<string> Check(CertificationWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationWindowAllowedLabelRule(List<string> Allowed) : ICertificationWindowRule
{
    public List<string> Check(CertificationWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification window.</summary>
public sealed record CertificationWindowValidator(List<ICertificationWindowRule> Rules)
{
    public List<string> Validate(CertificationWindow item)
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

    public void RequireValid(CertificationWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationWindow> items)
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

/// <summary>Functions of certification window rules.</summary>
public static class CertificationWindowRules
{
    public static CertificationWindowValidator DefaultCertificationWindowValidator()
    {
        return new CertificationWindowValidator(
            new List<ICertificationWindowRule>
            {
                new CertificationWindowLabelLengthRule(3600),
                new CertificationWindowRequiredCodeRule("certification window needs this field"),
                new CertificationWindowAllowedLabelRule(new List<string> { "scheduled", "capped" }),
            });
    }
}
