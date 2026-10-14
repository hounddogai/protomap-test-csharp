namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty group is in its life cycle.</summary>
public enum WarrantyGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A warranty group. Its code is unique within a tenant.</summary>
public sealed class WarrantyGroup(string code, string name, List<string> tags, int attempts, string employeeId, int windowDays, WarrantyGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public WarrantyGroupStatus Status { get; set; } = status;

    /// <summary>Starts a warranty group in status draft.</summary>
    public static WarrantyGroup NewWarrantyGroup(string code, string name)
    {
        return new WarrantyGroup(code, name, new List<string>(), 0, "", 0, WarrantyGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyGroupStatus next)
    {
        switch (Status)
        {
            case WarrantyGroupStatus.Draft:
                switch (next)
                {
                    case WarrantyGroupStatus.Suspended:
                        return true;
                }
                return false;
            case WarrantyGroupStatus.Pending:
                switch (next)
                {
                    case WarrantyGroupStatus.Closed:
                        return true;
                }
                return false;
            case WarrantyGroupStatus.Active:
                return next == WarrantyGroupStatus.Closed;
            case WarrantyGroupStatus.Suspended:
                return next == WarrantyGroupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WarrantyGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays + (double)Attempts, 2);
    }

    public WarrantyGroup Copy()
    {
        return new WarrantyGroup(Code, Name, Tags, Attempts, EmployeeId, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
