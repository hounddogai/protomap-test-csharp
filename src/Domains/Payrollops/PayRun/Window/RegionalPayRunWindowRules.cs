namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional pay run window must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayRunWindowRule
{
    List<string> Check(RegionalPayRunWindow item);
}

public sealed record RegionalPayRunWindowExpiresAtRangeRule(int Min, int Max) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPayRunWindowLabelLengthRule(int MaxLength) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunWindowLocaleLengthRule(int MaxLength) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunWindowRequiredCodeRule(string Message) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunWindowRequiredLabelRule(string Message) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayRunWindowAllowedCodeRule(List<string> Allowed) : IRegionalPayRunWindowRule
{
    public List<string> Check(RegionalPayRunWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pay run window.</summary>
public sealed record RegionalPayRunWindowValidator(List<IRegionalPayRunWindowRule> Rules)
{
    public List<string> Validate(RegionalPayRunWindow item)
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

    public void RequireValid(RegionalPayRunWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayRunWindow> items)
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

/// <summary>Functions of regional pay run window rules.</summary>
public static class RegionalPayRunWindowRules
{
    public static RegionalPayRunWindowValidator DefaultRegionalPayRunWindowValidator()
    {
        return new RegionalPayRunWindowValidator(
            new List<IRegionalPayRunWindowRule>
            {
                new RegionalPayRunWindowExpiresAtRangeRule(120, 86400),
                new RegionalPayRunWindowLabelLengthRule(80),
                new RegionalPayRunWindowLocaleLengthRule(86400),
                new RegionalPayRunWindowRequiredCodeRule("regional pay run window needs an entry"),
                new RegionalPayRunWindowRequiredLabelRule("regional pay run window needs this field"),
                new RegionalPayRunWindowAllowedCodeRule(new List<string>()),
            });
    }
}
