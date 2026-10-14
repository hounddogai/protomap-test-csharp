namespace Acme.Workforce.Contractors.Vendor.Score;

/// <summary>A rule a vendor score must pass. Check returns its problems, or none.</summary>
public interface IVendorScoreRule
{
    List<string> Check(VendorScore item);
}

public sealed record VendorScoreUpdatedAtRangeRule(int Min, int Max) : IVendorScoreRule
{
    public List<string> Check(VendorScore item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorScoreAttemptsRangeRule(int Min, int Max) : IVendorScoreRule
{
    public List<string> Check(VendorScore item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VendorScoreHoursRangeRule(double Min, double Max) : IVendorScoreRule
{
    public List<string> Check(VendorScore item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorScoreBudgetRangeRule(double Min, double Max) : IVendorScoreRule
{
    public List<string> Check(VendorScore item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VendorScoreCodeLengthRule(int MaxLength) : IVendorScoreRule
{
    public List<string> Check(VendorScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor score.</summary>
public sealed record VendorScoreValidator(List<IVendorScoreRule> Rules)
{
    public List<string> Validate(VendorScore item)
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

    public void RequireValid(VendorScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorScore> items)
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

/// <summary>Functions of vendor score rules.</summary>
public static class VendorScoreRules
{
    public static VendorScoreValidator DefaultVendorScoreValidator()
    {
        return new VendorScoreValidator(
            new List<IVendorScoreRule>
            {
                new VendorScoreUpdatedAtRangeRule(32, 1000),
                new VendorScoreAttemptsRangeRule(255, 86400),
                new VendorScoreHoursRangeRule(5.3, 4501.5),
                new VendorScoreBudgetRangeRule(2.5, 691.9),
                new VendorScoreCodeLengthRule(32),
            });
    }
}
