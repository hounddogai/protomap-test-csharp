namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted payslip manifest must pass. Check returns its problems, or none.</summary>
public interface IWeightedPayslipManifestRule
{
    List<string> Check(WeightedPayslipManifest item);
}

public sealed record WeightedPayslipManifestRequiredNameRule(string Message) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipManifestRequiredCodeRule(string Message) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipManifestBatchSizeRangeRule(int Min, int Max) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPayslipManifestNameLengthRule(int MaxLength) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipManifestAllowedCodeRule(List<string> Allowed) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipManifestDiscountRangeRule(double Min, double Max) : IWeightedPayslipManifestRule
{
    public List<string> Check(WeightedPayslipManifest item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted payslip manifest.</summary>
public sealed record WeightedPayslipManifestValidator(List<IWeightedPayslipManifestRule> Rules)
{
    public List<string> Validate(WeightedPayslipManifest item)
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

    public void RequireValid(WeightedPayslipManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPayslipManifest> items)
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

/// <summary>Functions of weighted payslip manifest rules.</summary>
public static class WeightedPayslipManifestRules
{
    public static WeightedPayslipManifestValidator DefaultWeightedPayslipManifestValidator()
    {
        return new WeightedPayslipManifestValidator(
            new List<IWeightedPayslipManifestRule>
            {
                new WeightedPayslipManifestRequiredNameRule("weighted payslip manifest needs this field"),
                new WeightedPayslipManifestRequiredCodeRule("weighted payslip manifest needs this field"),
                new WeightedPayslipManifestBatchSizeRangeRule(1000, 3600),
                new WeightedPayslipManifestNameLengthRule(64),
                new WeightedPayslipManifestAllowedCodeRule(new List<string> { "global", "adaptive" }),
                new WeightedPayslipManifestDiscountRangeRule(1.2, 727.0),
            });
    }
}
