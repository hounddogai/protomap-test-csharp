namespace Acme.Workforce.Absence.Balance.Draft;

/// <summary>A rule a pending balance template must pass. Check returns its problems, or none.</summary>
public interface IPendingBalanceTemplateRule
{
    List<string> Check(PendingBalanceTemplate item);
}

public sealed record PendingBalanceTemplateCodeLengthRule(int MaxLength) : IPendingBalanceTemplateRule
{
    public List<string> Check(PendingBalanceTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceTemplateLabelLengthRule(int MaxLength) : IPendingBalanceTemplateRule
{
    public List<string> Check(PendingBalanceTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceTemplateCodePrefixRule(string Prefix) : IPendingBalanceTemplateRule
{
    public List<string> Check(PendingBalanceTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBalanceTemplateLeaveDaysRangeRule(int Min, int Max) : IPendingBalanceTemplateRule
{
    public List<string> Check(PendingBalanceTemplate item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending balance template.</summary>
public sealed record PendingBalanceTemplateValidator(List<IPendingBalanceTemplateRule> Rules)
{
    public List<string> Validate(PendingBalanceTemplate item)
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

    public void RequireValid(PendingBalanceTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBalanceTemplate> items)
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

/// <summary>Functions of pending balance template rules.</summary>
public static class PendingBalanceTemplateRules
{
    public static PendingBalanceTemplateValidator DefaultPendingBalanceTemplateValidator()
    {
        return new PendingBalanceTemplateValidator(
            new List<IPendingBalanceTemplateRule>
            {
                new PendingBalanceTemplateCodeLengthRule(16),
                new PendingBalanceTemplateLabelLengthRule(16),
                new PendingBalanceTemplateCodePrefixRule("Q"),
                new PendingBalanceTemplateLeaveDaysRangeRule(64, 80),
            });
    }
}
