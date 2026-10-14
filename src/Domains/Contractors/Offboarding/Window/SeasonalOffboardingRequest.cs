namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal offboarding request is in its life cycle.</summary>
public enum SeasonalOffboardingRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal offboarding request, with what the planners need to act on it.</summary>
public sealed class SeasonalOffboardingRequest(string code, string label, double minimum, bool approved, string phone, double floorValue, double fte, double cost, string employeeId, string timezone, SeasonalOffboardingRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public double FloorValue { get; set; } = floorValue;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public string Timezone { get; set; } = timezone;
    public SeasonalOffboardingRequestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal offboarding request in status requested.</summary>
    public static SeasonalOffboardingRequest NewSeasonalOffboardingRequest(string code, string label)
    {
        return new SeasonalOffboardingRequest(
            code,
            label,
            0.0,
            false,
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            SeasonalOffboardingRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalOffboardingRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalOffboardingRequestStatus next)
    {
        switch (Status)
        {
            case SeasonalOffboardingRequestStatus.Requested:
                return next == SeasonalOffboardingRequestStatus.Revoked;
            case SeasonalOffboardingRequestStatus.Granted:
                switch (next)
                {
                    case SeasonalOffboardingRequestStatus.Revoked:
                    case SeasonalOffboardingRequestStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalOffboardingRequestStatus.Revoked:
                return next == SeasonalOffboardingRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalOffboardingRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal offboarding request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + Cost, 2);
    }

    public SeasonalOffboardingRequest Copy()
    {
        return new SeasonalOffboardingRequest(
            Code,
            Label,
            Minimum,
            Approved,
            Phone,
            FloorValue,
            Fte,
            Cost,
            EmployeeId,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
