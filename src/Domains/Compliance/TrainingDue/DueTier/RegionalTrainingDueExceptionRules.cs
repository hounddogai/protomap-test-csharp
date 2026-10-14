namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>A rule a regional training due exception must pass. Check returns its problems, or none.</summary>
public interface IRegionalTrainingDueExceptionRule
{
    List<string> Check(RegionalTrainingDueException item);
}

public sealed record RegionalTrainingDueExceptionRequiredCategoryRule(string Message) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueExceptionCodePrefixRule(string Prefix) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueExceptionCodeLengthRule(int MaxLength) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueExceptionFloorValueRangeRule(double Min, double Max) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalTrainingDueExceptionAttemptsRangeRule(int Min, int Max) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueExceptionRequiredTitleRule(string Message) : IRegionalTrainingDueExceptionRule
{
    public List<string> Check(RegionalTrainingDueException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional training due exception.</summary>
public sealed record RegionalTrainingDueExceptionValidator(List<IRegionalTrainingDueExceptionRule> Rules)
{
    public List<string> Validate(RegionalTrainingDueException item)
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

    public void RequireValid(RegionalTrainingDueException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTrainingDueException> items)
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

/// <summary>Functions of regional training due exception rules.</summary>
public static class RegionalTrainingDueExceptionRules
{
    public static RegionalTrainingDueExceptionValidator DefaultRegionalTrainingDueExceptionValidator()
    {
        return new RegionalTrainingDueExceptionValidator(
            new List<IRegionalTrainingDueExceptionRule>
            {
                new RegionalTrainingDueExceptionRequiredCategoryRule(
                    "regional training due exception needs an entry"),
                new RegionalTrainingDueExceptionCodePrefixRule("REF"),
                new RegionalTrainingDueExceptionCodeLengthRule(64),
                new RegionalTrainingDueExceptionFloorValueRangeRule(3.9, 1068.7),
                new RegionalTrainingDueExceptionAttemptsRangeRule(16, 64),
                new RegionalTrainingDueExceptionRequiredTitleRule("regional training due exception needs this field"),
            });
    }
}
