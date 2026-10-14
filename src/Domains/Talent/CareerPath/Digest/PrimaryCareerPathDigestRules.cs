namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>A rule a primary career path digest must pass. Check returns its problems, or none.</summary>
public interface IPrimaryCareerPathDigestRule
{
    List<string> Check(PrimaryCareerPathDigest item);
}

public sealed record PrimaryCareerPathDigestDepartmentLengthRule(int MaxLength) : IPrimaryCareerPathDigestRule
{
    public List<string> Check(PrimaryCareerPathDigest item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCareerPathDigestRequiredCodeRule(string Message) : IPrimaryCareerPathDigestRule
{
    public List<string> Check(PrimaryCareerPathDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCareerPathDigestMarginRangeRule(double Min, double Max) : IPrimaryCareerPathDigestRule
{
    public List<string> Check(PrimaryCareerPathDigest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryCareerPathDigestLabelLengthRule(int MaxLength) : IPrimaryCareerPathDigestRule
{
    public List<string> Check(PrimaryCareerPathDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryCareerPathDigestCodeLengthRule(int MaxLength) : IPrimaryCareerPathDigestRule
{
    public List<string> Check(PrimaryCareerPathDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary career path digest.</summary>
public sealed record PrimaryCareerPathDigestValidator(List<IPrimaryCareerPathDigestRule> Rules)
{
    public List<string> Validate(PrimaryCareerPathDigest item)
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

    public void RequireValid(PrimaryCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryCareerPathDigest> items)
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

/// <summary>Functions of primary career path digest rules.</summary>
public static class PrimaryCareerPathDigestRules
{
    public static PrimaryCareerPathDigestValidator DefaultPrimaryCareerPathDigestValidator()
    {
        return new PrimaryCareerPathDigestValidator(
            new List<IPrimaryCareerPathDigestRule>
            {
                new PrimaryCareerPathDigestDepartmentLengthRule(120),
                new PrimaryCareerPathDigestRequiredCodeRule("primary career path digest needs a value"),
                new PrimaryCareerPathDigestMarginRangeRule(2.6, 3791.0),
                new PrimaryCareerPathDigestLabelLengthRule(3600),
                new PrimaryCareerPathDigestCodeLengthRule(32),
            });
    }
}
