namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default laptop mapping is in its life cycle.</summary>
public enum DefaultLaptopMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default laptop mapping. Its code is unique within a tenant.</summary>
public sealed class DefaultLaptopMapping(string code, string name, bool archived, double discount, string notes, double rate, int tenureMonths, DefaultLaptopMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public DefaultLaptopMappingStatus Status { get; set; } = status;

    /// <summary>Starts a default laptop mapping in status proposed.</summary>
    public static DefaultLaptopMapping NewDefaultLaptopMapping(string code, string name)
    {
        return new DefaultLaptopMapping(code, name, false, 0.0, "", 0.0, 0, DefaultLaptopMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLaptopMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLaptopMappingStatus next)
    {
        switch (Status)
        {
            case DefaultLaptopMappingStatus.Proposed:
                return next == DefaultLaptopMappingStatus.Rejected;
            case DefaultLaptopMappingStatus.Approved:
                return next == DefaultLaptopMappingStatus.Rejected || next == DefaultLaptopMappingStatus.Withdrawn;
            case DefaultLaptopMappingStatus.Rejected:
                switch (next)
                {
                    case DefaultLaptopMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLaptopMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default laptop mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)TenureMonths - Rate, 2);
    }

    public DefaultLaptopMapping Copy()
    {
        return new DefaultLaptopMapping(Code, Name, Archived, Discount, Notes, Rate, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
