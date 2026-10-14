namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run group is in its life cycle.</summary>
public enum RegionalPayRunGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional pay run group and its current status.</summary>
public sealed class RegionalPayRunGroup(string code, string name, string contactEmail, double budget, double maximum, double floorValue, string summary, double fte, int quantity, string description, RegionalPayRunGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public RegionalPayRunGroupStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run group in status idle.</summary>
    public static RegionalPayRunGroup NewRegionalPayRunGroup(string code, string name)
    {
        return new RegionalPayRunGroup(
            code,
            name,
            "",
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            0,
            "",
            RegionalPayRunGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunGroupStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunGroupStatus.Idle:
                switch (next)
                {
                    case RegionalPayRunGroupStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPayRunGroupStatus.Warming:
                switch (next)
                {
                    case RegionalPayRunGroupStatus.Draining:
                    case RegionalPayRunGroupStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPayRunGroupStatus.Ready:
                switch (next)
                {
                    case RegionalPayRunGroupStatus.Draining:
                    case RegionalPayRunGroupStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPayRunGroupStatus.Draining:
                return next == RegionalPayRunGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue - (double)Quantity, 2);
    }

    public RegionalPayRunGroup Copy()
    {
        return new RegionalPayRunGroup(
            Code,
            Name,
            ContactEmail,
            Budget,
            Maximum,
            FloorValue,
            Summary,
            Fte,
            Quantity,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
