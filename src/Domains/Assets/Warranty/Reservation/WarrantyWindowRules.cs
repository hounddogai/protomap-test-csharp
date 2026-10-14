namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty window must pass. Check returns its problems, or none.</summary>
public interface IWarrantyWindowRule
{
    List<string> Check(WarrantyWindow item);
}

public sealed record WarrantyWindowLabelLengthRule(int MaxLength) : IWarrantyWindowRule
{
    public List<string> Check(WarrantyWindow item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyWindowRetriesRangeRule(int Min, int Max) : IWarrantyWindowRule
{
    public List<string> Check(WarrantyWindow item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyWindowRequiredChannelRule(string Message) : IWarrantyWindowRule
{
    public List<string> Check(WarrantyWindow item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyWindowAllowedCodeRule(List<string> Allowed) : IWarrantyWindowRule
{
    public List<string> Check(WarrantyWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty window.</summary>
public sealed record WarrantyWindowValidator(List<IWarrantyWindowRule> Rules)
{
    public List<string> Validate(WarrantyWindow item)
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

    public void RequireValid(WarrantyWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyWindow> items)
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

/// <summary>Functions of warranty window rules.</summary>
public static class WarrantyWindowRules
{
    public static WarrantyWindowValidator DefaultWarrantyWindowValidator()
    {
        return new WarrantyWindowValidator(
            new List<IWarrantyWindowRule>
            {
                new WarrantyWindowLabelLengthRule(3600),
                new WarrantyWindowRetriesRangeRule(32, 3600),
                new WarrantyWindowRequiredChannelRule("warranty window needs this field"),
                new WarrantyWindowAllowedCodeRule(new List<string> { "scheduled", "regional" }),
            });
    }
}
