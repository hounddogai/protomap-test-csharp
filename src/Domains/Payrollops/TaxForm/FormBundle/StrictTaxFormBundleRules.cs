namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>A rule a strict tax form bundle must pass. Check returns its problems, or none.</summary>
public interface IStrictTaxFormBundleRule
{
    List<string> Check(StrictTaxFormBundle item);
}

public sealed record StrictTaxFormBundleCodeLengthRule(int MaxLength) : IStrictTaxFormBundleRule
{
    public List<string> Check(StrictTaxFormBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormBundleTenureMonthsRangeRule(int Min, int Max) : IStrictTaxFormBundleRule
{
    public List<string> Check(StrictTaxFormBundle item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictTaxFormBundleTeamLengthRule(int MaxLength) : IStrictTaxFormBundleRule
{
    public List<string> Check(StrictTaxFormBundle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict tax form bundle.</summary>
public sealed record StrictTaxFormBundleValidator(List<IStrictTaxFormBundleRule> Rules)
{
    public List<string> Validate(StrictTaxFormBundle item)
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

    public void RequireValid(StrictTaxFormBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTaxFormBundle> items)
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

/// <summary>Functions of strict tax form bundle rules.</summary>
public static class StrictTaxFormBundleRules
{
    public static StrictTaxFormBundleValidator DefaultStrictTaxFormBundleValidator()
    {
        return new StrictTaxFormBundleValidator(
            new List<IStrictTaxFormBundleRule>
            {
                new StrictTaxFormBundleCodeLengthRule(1000),
                new StrictTaxFormBundleTenureMonthsRangeRule(16, 80),
                new StrictTaxFormBundleTeamLengthRule(32),
            });
    }
}
