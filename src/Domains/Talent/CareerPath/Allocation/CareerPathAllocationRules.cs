namespace Acme.Workforce.Talent.CareerPath.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path allocation must pass. Check returns its problems, or none.</summary>
public interface ICareerPathAllocationRule
{
    List<string> Check(CareerPathAllocation item);
}

public sealed record CareerPathAllocationRequiredCodeRule(string Message) : ICareerPathAllocationRule
{
    public List<string> Check(CareerPathAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAllocationAllowedLabelRule(List<string> Allowed) : ICareerPathAllocationRule
{
    public List<string> Check(CareerPathAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAllocationRequiredTimezoneRule(string Message) : ICareerPathAllocationRule
{
    public List<string> Check(CareerPathAllocation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAllocationChannelLengthRule(int MaxLength) : ICareerPathAllocationRule
{
    public List<string> Check(CareerPathAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path allocation.</summary>
public sealed record CareerPathAllocationValidator(List<ICareerPathAllocationRule> Rules)
{
    public List<string> Validate(CareerPathAllocation item)
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

    public void RequireValid(CareerPathAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathAllocation> items)
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

/// <summary>Functions of career path allocation rules.</summary>
public static class CareerPathAllocationRules
{
    public static CareerPathAllocationValidator DefaultCareerPathAllocationValidator()
    {
        return new CareerPathAllocationValidator(
            new List<ICareerPathAllocationRule>
            {
                new CareerPathAllocationRequiredCodeRule("career path allocation needs an entry"),
                new CareerPathAllocationAllowedLabelRule(new List<string>()),
                new CareerPathAllocationRequiredTimezoneRule("career path allocation needs a value"),
                new CareerPathAllocationChannelLengthRule(255),
            });
    }
}
