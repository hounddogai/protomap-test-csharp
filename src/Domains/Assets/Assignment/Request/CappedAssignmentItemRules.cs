namespace Acme.Workforce.Assets.Assignment.Request;

/// <summary>A rule a capped assignment item must pass. Check returns its problems, or none.</summary>
public interface ICappedAssignmentItemRule
{
    List<string> Check(CappedAssignmentItem item);
}

public sealed record CappedAssignmentItemCodePrefixRule(string Prefix) : ICappedAssignmentItemRule
{
    public List<string> Check(CappedAssignmentItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssignmentItemScoreRangeRule(double Min, double Max) : ICappedAssignmentItemRule
{
    public List<string> Check(CappedAssignmentItem item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAssignmentItemRequiredRegionRule(string Message) : ICappedAssignmentItemRule
{
    public List<string> Check(CappedAssignmentItem item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAssignmentItemDepartmentLengthRule(int MaxLength) : ICappedAssignmentItemRule
{
    public List<string> Check(CappedAssignmentItem item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped assignment item.</summary>
public sealed record CappedAssignmentItemValidator(List<ICappedAssignmentItemRule> Rules)
{
    public List<string> Validate(CappedAssignmentItem item)
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

    public void RequireValid(CappedAssignmentItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAssignmentItem> items)
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

/// <summary>Functions of capped assignment item rules.</summary>
public static class CappedAssignmentItemRules
{
    public static CappedAssignmentItemValidator DefaultCappedAssignmentItemValidator()
    {
        return new CappedAssignmentItemValidator(
            new List<ICappedAssignmentItemRule>
            {
                new CappedAssignmentItemCodePrefixRule("REF"),
                new CappedAssignmentItemScoreRangeRule(5.2, 2024.0),
                new CappedAssignmentItemRequiredRegionRule("capped assignment item needs an entry"),
                new CappedAssignmentItemDepartmentLengthRule(255),
            });
    }
}
