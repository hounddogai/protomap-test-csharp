namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal license seat preference is in its life cycle.</summary>
public enum SeasonalLicenseSeatPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal license seat preference. Its code is unique within a tenant.</summary>
public sealed class SeasonalLicenseSeatPreference(string code, string name, string region, int quantity, int endsAt, string employeeId, SeasonalLicenseSeatPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public SeasonalLicenseSeatPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal license seat preference in status proposed.</summary>
    public static SeasonalLicenseSeatPreference NewSeasonalLicenseSeatPreference(string code, string name)
    {
        return new SeasonalLicenseSeatPreference(
            code,
            name,
            "",
            0,
            0,
            "",
            SeasonalLicenseSeatPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLicenseSeatPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLicenseSeatPreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalLicenseSeatPreferenceStatus.Proposed:
                return next == SeasonalLicenseSeatPreferenceStatus.Rejected;
            case SeasonalLicenseSeatPreferenceStatus.Approved:
                switch (next)
                {
                    case SeasonalLicenseSeatPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatPreferenceStatus.Rejected:
                switch (next)
                {
                    case SeasonalLicenseSeatPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalLicenseSeatPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal license seat preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt * (double)Quantity, 2);
    }

    public SeasonalLicenseSeatPreference Copy()
    {
        return new SeasonalLicenseSeatPreference(Code, Name, Region, Quantity, EndsAt, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
