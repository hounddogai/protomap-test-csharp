namespace Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>A rule a manual balance summary must pass. Check returns its problems, or none.</summary>
public interface IManualBalanceSummaryRule
{
    List<string> Check(ManualBalanceSummary item);
}

public sealed record ManualBalanceSummaryRequiredCodeRule(string Message) : IManualBalanceSummaryRule
{
    public List<string> Check(ManualBalanceSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceSummaryRequiredGradeRule(string Message) : IManualBalanceSummaryRule
{
    public List<string> Check(ManualBalanceSummary item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceSummaryLabelLengthRule(int MaxLength) : IManualBalanceSummaryRule
{
    public List<string> Check(ManualBalanceSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceSummaryGradeLengthRule(int MaxLength) : IManualBalanceSummaryRule
{
    public List<string> Check(ManualBalanceSummary item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBalanceSummaryRequiredCurrencyRule(string Message) : IManualBalanceSummaryRule
{
    public List<string> Check(ManualBalanceSummary item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual balance summary.</summary>
public sealed record ManualBalanceSummaryValidator(List<IManualBalanceSummaryRule> Rules)
{
    public List<string> Validate(ManualBalanceSummary item)
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

    public void RequireValid(ManualBalanceSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBalanceSummary> items)
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

/// <summary>Functions of manual balance summary rules.</summary>
public static class ManualBalanceSummaryRules
{
    public static ManualBalanceSummaryValidator DefaultManualBalanceSummaryValidator()
    {
        return new ManualBalanceSummaryValidator(
            new List<IManualBalanceSummaryRule>
            {
                new ManualBalanceSummaryRequiredCodeRule("manual balance summary needs this field"),
                new ManualBalanceSummaryRequiredGradeRule("manual balance summary needs a value"),
                new ManualBalanceSummaryLabelLengthRule(120),
                new ManualBalanceSummaryGradeLengthRule(120),
                new ManualBalanceSummaryRequiredCurrencyRule("manual balance summary needs an entry"),
            });
    }
}
