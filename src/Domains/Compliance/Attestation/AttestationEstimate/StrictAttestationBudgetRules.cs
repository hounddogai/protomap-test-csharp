namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>A rule a strict attestation budget must pass. Check returns its problems, or none.</summary>
public interface IStrictAttestationBudgetRule
{
    List<string> Check(StrictAttestationBudget item);
}

public sealed record StrictAttestationBudgetLabelLengthRule(int MaxLength) : IStrictAttestationBudgetRule
{
    public List<string> Check(StrictAttestationBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationBudgetRequiredNotesRule(string Message) : IStrictAttestationBudgetRule
{
    public List<string> Check(StrictAttestationBudget item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationBudgetCodePrefixRule(string Prefix) : IStrictAttestationBudgetRule
{
    public List<string> Check(StrictAttestationBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationBudgetLimitRangeRule(int Min, int Max) : IStrictAttestationBudgetRule
{
    public List<string> Check(StrictAttestationBudget item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAttestationBudgetFloorValueRangeRule(double Min, double Max) : IStrictAttestationBudgetRule
{
    public List<string> Check(StrictAttestationBudget item)
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

/// <summary>Runs every rule of a strict attestation budget.</summary>
public sealed record StrictAttestationBudgetValidator(List<IStrictAttestationBudgetRule> Rules)
{
    public List<string> Validate(StrictAttestationBudget item)
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

    public void RequireValid(StrictAttestationBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAttestationBudget> items)
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

/// <summary>Functions of strict attestation budget rules.</summary>
public static class StrictAttestationBudgetRules
{
    public static StrictAttestationBudgetValidator DefaultStrictAttestationBudgetValidator()
    {
        return new StrictAttestationBudgetValidator(
            new List<IStrictAttestationBudgetRule>
            {
                new StrictAttestationBudgetLabelLengthRule(64),
                new StrictAttestationBudgetRequiredNotesRule("strict attestation budget needs this field"),
                new StrictAttestationBudgetCodePrefixRule("OPS"),
                new StrictAttestationBudgetLimitRangeRule(32, 3600),
                new StrictAttestationBudgetFloorValueRangeRule(5.3, 1768.3),
            });
    }
}
