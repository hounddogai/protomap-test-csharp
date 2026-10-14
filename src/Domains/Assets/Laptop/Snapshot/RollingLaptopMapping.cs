namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling laptop mapping is in its life cycle.</summary>
public enum RollingLaptopMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rolling laptop mapping as the laptop team keeps it.</summary>
public sealed class RollingLaptopMapping(string code, string name, List<string> tags, string ownerId, string skill, double amount, RollingLaptopMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public double Amount { get; set; } = amount;
    public RollingLaptopMappingStatus Status { get; set; } = status;

    /// <summary>Starts a rolling laptop mapping in status requested.</summary>
    public static RollingLaptopMapping NewRollingLaptopMapping(string code, string name)
    {
        return new RollingLaptopMapping(
            code,
            name,
            new List<string>(),
            "",
            "",
            0.0,
            RollingLaptopMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLaptopMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLaptopMappingStatus next)
    {
        switch (Status)
        {
            case RollingLaptopMappingStatus.Requested:
                switch (next)
                {
                    case RollingLaptopMappingStatus.Revoked:
                    case RollingLaptopMappingStatus.Expired:
                        return true;
                }
                return false;
            case RollingLaptopMappingStatus.Granted:
                switch (next)
                {
                    case RollingLaptopMappingStatus.Expired:
                        return true;
                }
                return false;
            case RollingLaptopMappingStatus.Revoked:
                switch (next)
                {
                    case RollingLaptopMappingStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLaptopMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling laptop mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling laptop mapping"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RollingLaptopMapping Copy()
    {
        return new RollingLaptopMapping(Code, Name, Tags, OwnerId, Skill, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
