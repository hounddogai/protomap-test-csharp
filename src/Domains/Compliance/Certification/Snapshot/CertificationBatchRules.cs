namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification batch must pass. Check returns its problems, or none.</summary>
public interface ICertificationBatchRule
{
    List<string> Check(CertificationBatch item);
}

public sealed record CertificationBatchAllowedTitleRule(List<string> Allowed) : ICertificationBatchRule
{
    public List<string> Check(CertificationBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record CertificationBatchActiveGuardRule(double Limit) : ICertificationBatchRule
{
    public List<string> Check(CertificationBatch item)
    {
        if (item.Active && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record CertificationBatchMinimumRangeRule(double Min, double Max) : ICertificationBatchRule
{
    public List<string> Check(CertificationBatch item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a certification batch.</summary>
public sealed record CertificationBatchValidator(List<ICertificationBatchRule> Rules)
{
    public List<string> Validate(CertificationBatch item)
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

    public void RequireValid(CertificationBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationBatch> items)
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

/// <summary>Functions of certification batch rules.</summary>
public static class CertificationBatchRules
{
    public static CertificationBatchValidator DefaultCertificationBatchValidator()
    {
        return new CertificationBatchValidator(
            new List<ICertificationBatchRule>
            {
                new CertificationBatchAllowedTitleRule(new List<string> { "manual", "batched", "strict" }),
                new CertificationBatchActiveGuardRule(0.7),
                new CertificationBatchMinimumRangeRule(3.1, 2077.4),
            });
    }
}
