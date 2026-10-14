namespace Acme.Workforce.Assets.Laptop.Window;

/// <summary>A rule a global laptop draft must pass. Check returns its problems, or none.</summary>
public interface IGlobalLaptopDraftRule
{
    List<string> Check(GlobalLaptopDraft item);
}

public sealed record GlobalLaptopDraftBatchSizeRangeRule(int Min, int Max) : IGlobalLaptopDraftRule
{
    public List<string> Check(GlobalLaptopDraft item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopDraftCodePrefixRule(string Prefix) : IGlobalLaptopDraftRule
{
    public List<string> Check(GlobalLaptopDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopDraftRequiredExternalRefRule(string Message) : IGlobalLaptopDraftRule
{
    public List<string> Check(GlobalLaptopDraft item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopDraftLabelLengthRule(int MaxLength) : IGlobalLaptopDraftRule
{
    public List<string> Check(GlobalLaptopDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global laptop draft.</summary>
public sealed record GlobalLaptopDraftValidator(List<IGlobalLaptopDraftRule> Rules)
{
    public List<string> Validate(GlobalLaptopDraft item)
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

    public void RequireValid(GlobalLaptopDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalLaptopDraft> items)
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

/// <summary>Functions of global laptop draft rules.</summary>
public static class GlobalLaptopDraftRules
{
    public static GlobalLaptopDraftValidator DefaultGlobalLaptopDraftValidator()
    {
        return new GlobalLaptopDraftValidator(
            new List<IGlobalLaptopDraftRule>
            {
                new GlobalLaptopDraftBatchSizeRangeRule(32, 255),
                new GlobalLaptopDraftCodePrefixRule("OPS"),
                new GlobalLaptopDraftRequiredExternalRefRule("global laptop draft needs a value"),
                new GlobalLaptopDraftLabelLengthRule(120),
            });
    }
}
