namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>A rule a manual reorg profile must pass. Check returns its problems, or none.</summary>
public interface IManualReorgProfileRule
{
    List<string> Check(ManualReorgProfile item);
}

public sealed record ManualReorgProfileCodeLengthRule(int MaxLength) : IManualReorgProfileRule
{
    public List<string> Check(ManualReorgProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgProfileRequiredNameRule(string Message) : IManualReorgProfileRule
{
    public List<string> Check(ManualReorgProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgProfileNameLengthRule(int MaxLength) : IManualReorgProfileRule
{
    public List<string> Check(ManualReorgProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgProfileRequiredCodeRule(string Message) : IManualReorgProfileRule
{
    public List<string> Check(ManualReorgProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual reorg profile.</summary>
public sealed record ManualReorgProfileValidator(List<IManualReorgProfileRule> Rules)
{
    public List<string> Validate(ManualReorgProfile item)
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

    public void RequireValid(ManualReorgProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualReorgProfile> items)
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

/// <summary>Functions of manual reorg profile rules.</summary>
public static class ManualReorgProfileRules
{
    public static ManualReorgProfileValidator DefaultManualReorgProfileValidator()
    {
        return new ManualReorgProfileValidator(
            new List<IManualReorgProfileRule>
            {
                new ManualReorgProfileCodeLengthRule(3600),
                new ManualReorgProfileRequiredNameRule("manual reorg profile needs a value"),
                new ManualReorgProfileNameLengthRule(120),
                new ManualReorgProfileRequiredCodeRule("manual reorg profile needs this field"),
            });
    }
}
