namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global vendor slot must pass. Check returns its problems, or none.</summary>
public interface IGlobalVendorSlotRule
{
    List<string> Check(GlobalVendorSlot item);
}

public sealed record GlobalVendorSlotDepartmentLengthRule(int MaxLength) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorSlotRequiredCodeRule(string Message) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorSlotRequiredDepartmentRule(string Message) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorSlotAllowedCodeRule(List<string> Allowed) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorSlotLabelLengthRule(int MaxLength) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorSlotGradeLengthRule(int MaxLength) : IGlobalVendorSlotRule
{
    public List<string> Check(GlobalVendorSlot item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global vendor slot.</summary>
public sealed record GlobalVendorSlotValidator(List<IGlobalVendorSlotRule> Rules)
{
    public List<string> Validate(GlobalVendorSlot item)
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

    public void RequireValid(GlobalVendorSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalVendorSlot> items)
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

/// <summary>Functions of global vendor slot rules.</summary>
public static class GlobalVendorSlotRules
{
    public static GlobalVendorSlotValidator DefaultGlobalVendorSlotValidator()
    {
        return new GlobalVendorSlotValidator(
            new List<IGlobalVendorSlotRule>
            {
                new GlobalVendorSlotDepartmentLengthRule(32),
                new GlobalVendorSlotRequiredCodeRule("global vendor slot needs an entry"),
                new GlobalVendorSlotRequiredDepartmentRule("global vendor slot needs this field"),
                new GlobalVendorSlotAllowedCodeRule(new List<string> { "default" }),
                new GlobalVendorSlotLabelLengthRule(80),
                new GlobalVendorSlotGradeLengthRule(120),
            });
    }
}
