namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict accrual forecast must pass. Check returns its problems, or none.</summary>
public interface IStrictAccrualForecastRule
{
    List<string> Check(StrictAccrualForecast item);
}

public sealed record StrictAccrualForecastNameLengthRule(int MaxLength) : IStrictAccrualForecastRule
{
    public List<string> Check(StrictAccrualForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualForecastRequiredNameRule(string Message) : IStrictAccrualForecastRule
{
    public List<string> Check(StrictAccrualForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualForecastAllowedContactEmailRule(List<string> Allowed) : IStrictAccrualForecastRule
{
    public List<string> Check(StrictAccrualForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict accrual forecast.</summary>
public sealed record StrictAccrualForecastValidator(List<IStrictAccrualForecastRule> Rules)
{
    public List<string> Validate(StrictAccrualForecast item)
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

    public void RequireValid(StrictAccrualForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccrualForecast> items)
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

/// <summary>Functions of strict accrual forecast rules.</summary>
public static class StrictAccrualForecastRules
{
    public static StrictAccrualForecastValidator DefaultStrictAccrualForecastValidator()
    {
        return new StrictAccrualForecastValidator(
            new List<IStrictAccrualForecastRule>
            {
                new StrictAccrualForecastNameLengthRule(16),
                new StrictAccrualForecastRequiredNameRule("strict accrual forecast needs an entry"),
                new StrictAccrualForecastAllowedContactEmailRule(
                    new List<string> { "fallback", "legacy", "pending" }),
            });
    }
}
