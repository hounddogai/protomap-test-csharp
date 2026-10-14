namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>A rule a adaptive training due exception must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTrainingDueExceptionRule
{
    List<string> Check(AdaptiveTrainingDueException item);
}

public sealed record AdaptiveTrainingDueExceptionMaximumRangeRule(double Min, double Max) : IAdaptiveTrainingDueExceptionRule
{
    public List<string> Check(AdaptiveTrainingDueException item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTrainingDueExceptionRequiredCodeRule(string Message) : IAdaptiveTrainingDueExceptionRule
{
    public List<string> Check(AdaptiveTrainingDueException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDueExceptionContactEmailLengthRule(int MaxLength) : IAdaptiveTrainingDueExceptionRule
{
    public List<string> Check(AdaptiveTrainingDueException item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDueExceptionCostRangeRule(double Min, double Max) : IAdaptiveTrainingDueExceptionRule
{
    public List<string> Check(AdaptiveTrainingDueException item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTrainingDueExceptionCodeLengthRule(int MaxLength) : IAdaptiveTrainingDueExceptionRule
{
    public List<string> Check(AdaptiveTrainingDueException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive training due exception.</summary>
public sealed record AdaptiveTrainingDueExceptionValidator(List<IAdaptiveTrainingDueExceptionRule> Rules)
{
    public List<string> Validate(AdaptiveTrainingDueException item)
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

    public void RequireValid(AdaptiveTrainingDueException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTrainingDueException> items)
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

/// <summary>Functions of adaptive training due exception rules.</summary>
public static class AdaptiveTrainingDueExceptionRules
{
    public static AdaptiveTrainingDueExceptionValidator DefaultAdaptiveTrainingDueExceptionValidator()
    {
        return new AdaptiveTrainingDueExceptionValidator(
            new List<IAdaptiveTrainingDueExceptionRule>
            {
                new AdaptiveTrainingDueExceptionMaximumRangeRule(9.5, 3391.6),
                new AdaptiveTrainingDueExceptionRequiredCodeRule("adaptive training due exception needs a value"),
                new AdaptiveTrainingDueExceptionContactEmailLengthRule(86400),
                new AdaptiveTrainingDueExceptionCostRangeRule(4.2, 4808.7),
                new AdaptiveTrainingDueExceptionCodeLengthRule(1000),
            });
    }
}
