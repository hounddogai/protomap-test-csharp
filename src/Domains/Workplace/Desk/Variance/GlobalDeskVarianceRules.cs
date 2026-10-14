namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>A rule a global desk variance must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeskVarianceRule
{
    List<string> Check(GlobalDeskVariance item);
}

public sealed record GlobalDeskVarianceQuantityRangeRule(int Min, int Max) : IGlobalDeskVarianceRule
{
    public List<string> Check(GlobalDeskVariance item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalDeskVarianceLabelLengthRule(int MaxLength) : IGlobalDeskVarianceRule
{
    public List<string> Check(GlobalDeskVariance item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskVarianceCodePrefixRule(string Prefix) : IGlobalDeskVarianceRule
{
    public List<string> Check(GlobalDeskVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskVarianceRequiredEmployeeIdRule(string Message) : IGlobalDeskVarianceRule
{
    public List<string> Check(GlobalDeskVariance item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeskVarianceRequiredSalaryBandRule(string Message) : IGlobalDeskVarianceRule
{
    public List<string> Check(GlobalDeskVariance item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global desk variance.</summary>
public sealed record GlobalDeskVarianceValidator(List<IGlobalDeskVarianceRule> Rules)
{
    public List<string> Validate(GlobalDeskVariance item)
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

    public void RequireValid(GlobalDeskVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeskVariance> items)
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

/// <summary>Functions of global desk variance rules.</summary>
public static class GlobalDeskVarianceRules
{
    public static GlobalDeskVarianceValidator DefaultGlobalDeskVarianceValidator()
    {
        return new GlobalDeskVarianceValidator(
            new List<IGlobalDeskVarianceRule>
            {
                new GlobalDeskVarianceQuantityRangeRule(16, 64),
                new GlobalDeskVarianceLabelLengthRule(120),
                new GlobalDeskVarianceCodePrefixRule("X"),
                new GlobalDeskVarianceRequiredEmployeeIdRule("global desk variance needs a value"),
                new GlobalDeskVarianceRequiredSalaryBandRule("global desk variance needs a value"),
            });
    }
}
