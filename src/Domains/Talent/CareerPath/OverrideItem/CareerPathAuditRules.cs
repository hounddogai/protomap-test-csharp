namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>A rule a career path audit must pass. Check returns its problems, or none.</summary>
public interface ICareerPathAuditRule
{
    List<string> Check(CareerPathAudit item);
}

public sealed record CareerPathAuditCodeLengthRule(int MaxLength) : ICareerPathAuditRule
{
    public List<string> Check(CareerPathAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAuditCodePrefixRule(string Prefix) : ICareerPathAuditRule
{
    public List<string> Check(CareerPathAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAuditSequenceRangeRule(int Min, int Max) : ICareerPathAuditRule
{
    public List<string> Check(CareerPathAudit item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path audit.</summary>
public sealed record CareerPathAuditValidator(List<ICareerPathAuditRule> Rules)
{
    public List<string> Validate(CareerPathAudit item)
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

    public void RequireValid(CareerPathAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathAudit> items)
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

/// <summary>Functions of career path audit rules.</summary>
public static class CareerPathAuditRules
{
    public static CareerPathAuditValidator DefaultCareerPathAuditValidator()
    {
        return new CareerPathAuditValidator(
            new List<ICareerPathAuditRule>
            {
                new CareerPathAuditCodeLengthRule(1000),
                new CareerPathAuditCodePrefixRule("X"),
                new CareerPathAuditSequenceRangeRule(16, 32),
            });
    }
}
