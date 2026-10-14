namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday policy is in its life cycle.</summary>
public enum HolidayPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A holiday policy as the holiday team keeps it.</summary>
public sealed class HolidayPolicy(string code, string name, string location, double fte, string channel, HolidayPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public HolidayPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a holiday policy in status created.</summary>
    public static HolidayPolicy NewHolidayPolicy(string code, string name)
    {
        return new HolidayPolicy(code, name, "", 0.0, "", HolidayPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayPolicyStatus next)
    {
        switch (Status)
        {
            case HolidayPolicyStatus.Created:
                return next == HolidayPolicyStatus.Validated || next == HolidayPolicyStatus.Published;
            case HolidayPolicyStatus.Validated:
                return next == HolidayPolicyStatus.Retired;
            case HolidayPolicyStatus.Published:
                switch (next)
                {
                    case HolidayPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday policy"} {Code}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public HolidayPolicy Copy()
    {
        return new HolidayPolicy(Code, Name, Location, Fte, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
