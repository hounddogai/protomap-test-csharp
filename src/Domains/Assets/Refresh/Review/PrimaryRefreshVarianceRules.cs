namespace Acme.Workforce.Assets.Refresh.Review;

/// <summary>A rule a primary refresh variance must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRefreshVarianceRule
{
    List<string> Check(PrimaryRefreshVariance item);
}

public sealed record PrimaryRefreshVarianceLabelLengthRule(int MaxLength) : IPrimaryRefreshVarianceRule
{
    public List<string> Check(PrimaryRefreshVariance item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshVarianceGradeLengthRule(int MaxLength) : IPrimaryRefreshVarianceRule
{
    public List<string> Check(PrimaryRefreshVariance item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRefreshVarianceStartsAtRangeRule(int Min, int Max) : IPrimaryRefreshVarianceRule
{
    public List<string> Check(PrimaryRefreshVariance item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryRefreshVarianceRequiredGradeRule(string Message) : IPrimaryRefreshVarianceRule
{
    public List<string> Check(PrimaryRefreshVariance item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary refresh variance.</summary>
public sealed record PrimaryRefreshVarianceValidator(List<IPrimaryRefreshVarianceRule> Rules)
{
    public List<string> Validate(PrimaryRefreshVariance item)
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

    public void RequireValid(PrimaryRefreshVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRefreshVariance> items)
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

/// <summary>Functions of primary refresh variance rules.</summary>
public static class PrimaryRefreshVarianceRules
{
    public static PrimaryRefreshVarianceValidator DefaultPrimaryRefreshVarianceValidator()
    {
        return new PrimaryRefreshVarianceValidator(
            new List<IPrimaryRefreshVarianceRule>
            {
                new PrimaryRefreshVarianceLabelLengthRule(16),
                new PrimaryRefreshVarianceGradeLengthRule(120),
                new PrimaryRefreshVarianceStartsAtRangeRule(120, 1000),
                new PrimaryRefreshVarianceRequiredGradeRule("primary refresh variance needs a value"),
            });
    }
}
