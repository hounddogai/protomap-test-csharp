namespace Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>A rule a weighted payslip draft must pass. Check returns its problems, or none.</summary>
public interface IWeightedPayslipDraftRule
{
    List<string> Check(WeightedPayslipDraft item);
}

public sealed record WeightedPayslipDraftBudgetRangeRule(double Min, double Max) : IWeightedPayslipDraftRule
{
    public List<string> Check(WeightedPayslipDraft item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipDraftManagerIdLengthRule(int MaxLength) : IWeightedPayslipDraftRule
{
    public List<string> Check(WeightedPayslipDraft item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipDraftRequiredManagerIdRule(string Message) : IWeightedPayslipDraftRule
{
    public List<string> Check(WeightedPayslipDraft item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipDraftCodePrefixRule(string Prefix) : IWeightedPayslipDraftRule
{
    public List<string> Check(WeightedPayslipDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayslipDraftLabelLengthRule(int MaxLength) : IWeightedPayslipDraftRule
{
    public List<string> Check(WeightedPayslipDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted payslip draft.</summary>
public sealed record WeightedPayslipDraftValidator(List<IWeightedPayslipDraftRule> Rules)
{
    public List<string> Validate(WeightedPayslipDraft item)
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

    public void RequireValid(WeightedPayslipDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPayslipDraft> items)
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

/// <summary>Functions of weighted payslip draft rules.</summary>
public static class WeightedPayslipDraftRules
{
    public static WeightedPayslipDraftValidator DefaultWeightedPayslipDraftValidator()
    {
        return new WeightedPayslipDraftValidator(
            new List<IWeightedPayslipDraftRule>
            {
                new WeightedPayslipDraftBudgetRangeRule(2.0, 643.0),
                new WeightedPayslipDraftManagerIdLengthRule(3600),
                new WeightedPayslipDraftRequiredManagerIdRule("weighted payslip draft needs this field"),
                new WeightedPayslipDraftCodePrefixRule("OPS"),
                new WeightedPayslipDraftLabelLengthRule(16),
            });
    }
}
