namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>A rule a certification snapshot must pass. Check returns its problems, or none.</summary>
public interface ICertificationSnapshotRule
{
    List<string> Check(CertificationSnapshot item);
}

public sealed record CertificationSnapshotRequiredSourceRule(string Message) : ICertificationSnapshotRule
{
    public List<string> Check(CertificationSnapshot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationSnapshotSourceLengthRule(int MaxLength) : ICertificationSnapshotRule
{
    public List<string> Check(CertificationSnapshot item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationSnapshotHoursRangeRule(double Min, double Max) : ICertificationSnapshotRule
{
    public List<string> Check(CertificationSnapshot item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a certification snapshot.</summary>
public sealed record CertificationSnapshotValidator(List<ICertificationSnapshotRule> Rules)
{
    public List<string> Validate(CertificationSnapshot item)
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

    public void RequireValid(CertificationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationSnapshot> items)
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

/// <summary>Functions of certification snapshot rules.</summary>
public static class CertificationSnapshotRules
{
    public static CertificationSnapshotValidator DefaultCertificationSnapshotValidator()
    {
        return new CertificationSnapshotValidator(
            new List<ICertificationSnapshotRule>
            {
                new CertificationSnapshotRequiredSourceRule("certification snapshot needs an entry"),
                new CertificationSnapshotSourceLengthRule(120),
                new CertificationSnapshotHoursRangeRule(5.7, 2335.9),
            });
    }
}
