namespace Acme.Workforce.Engagement.Recognition.Ledger;

/// <summary>A rule a recognition baseline must pass. Check returns its problems, or none.</summary>
public interface IRecognitionBaselineRule
{
    List<string> Check(RecognitionBaseline item);
}

public sealed record RecognitionBaselineNameLengthRule(int MaxLength) : IRecognitionBaselineRule
{
    public List<string> Check(RecognitionBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBaselineCostCenterLengthRule(int MaxLength) : IRecognitionBaselineRule
{
    public List<string> Check(RecognitionBaseline item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBaselineCodeLengthRule(int MaxLength) : IRecognitionBaselineRule
{
    public List<string> Check(RecognitionBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBaselineRequiredCostCenterRule(string Message) : IRecognitionBaselineRule
{
    public List<string> Check(RecognitionBaseline item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition baseline.</summary>
public sealed record RecognitionBaselineValidator(List<IRecognitionBaselineRule> Rules)
{
    public List<string> Validate(RecognitionBaseline item)
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

    public void RequireValid(RecognitionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionBaseline> items)
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

/// <summary>Functions of recognition baseline rules.</summary>
public static class RecognitionBaselineRules
{
    public static RecognitionBaselineValidator DefaultRecognitionBaselineValidator()
    {
        return new RecognitionBaselineValidator(
            new List<IRecognitionBaselineRule>
            {
                new RecognitionBaselineNameLengthRule(86400),
                new RecognitionBaselineCostCenterLengthRule(255),
                new RecognitionBaselineCodeLengthRule(1000),
                new RecognitionBaselineRequiredCostCenterRule("recognition baseline needs a value"),
            });
    }
}
