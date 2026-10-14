namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk forecast must pass. Check returns its problems, or none.</summary>
public interface IDeskForecastRule
{
    List<string> Check(DeskForecast item);
}

public sealed record DeskForecastPinnedGuardRule(int Limit) : IDeskForecastRule
{
    public List<string> Check(DeskForecast item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DeskForecastAllowedCodeRule(List<string> Allowed) : IDeskForecastRule
{
    public List<string> Check(DeskForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record DeskForecastTitleLengthRule(int MaxLength) : IDeskForecastRule
{
    public List<string> Check(DeskForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskForecastCodeLengthRule(int MaxLength) : IDeskForecastRule
{
    public List<string> Check(DeskForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk forecast.</summary>
public sealed record DeskForecastValidator(List<IDeskForecastRule> Rules)
{
    public List<string> Validate(DeskForecast item)
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

    public void RequireValid(DeskForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskForecast> items)
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

/// <summary>Functions of desk forecast rules.</summary>
public static class DeskForecastRules
{
    public static DeskForecastValidator DefaultDeskForecastValidator()
    {
        return new DeskForecastValidator(
            new List<IDeskForecastRule>
            {
                new DeskForecastPinnedGuardRule(64),
                new DeskForecastAllowedCodeRule(new List<string>()),
                new DeskForecastTitleLengthRule(1000),
                new DeskForecastCodeLengthRule(120),
            });
    }
}
