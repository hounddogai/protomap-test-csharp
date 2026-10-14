namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted license seat estimate is in its life cycle.</summary>
public enum WeightedLicenseSeatEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted license seat estimate. Its code is unique within a tenant.</summary>
public sealed class WeightedLicenseSeatEstimate(string code, string name, int capacity, string contactEmail, int updatedAt, string salaryBand, string timezone, string managerId, WeightedLicenseSeatEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public WeightedLicenseSeatEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted license seat estimate in status proposed.</summary>
    public static WeightedLicenseSeatEstimate NewWeightedLicenseSeatEstimate(string code, string name)
    {
        return new WeightedLicenseSeatEstimate(
            code,
            name,
            0,
            "",
            0,
            "",
            "",
            "",
            WeightedLicenseSeatEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLicenseSeatEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLicenseSeatEstimateStatus next)
    {
        switch (Status)
        {
            case WeightedLicenseSeatEstimateStatus.Proposed:
                switch (next)
                {
                    case WeightedLicenseSeatEstimateStatus.Rejected:
                    case WeightedLicenseSeatEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedLicenseSeatEstimateStatus.Approved:
                return next == WeightedLicenseSeatEstimateStatus.Rejected;
            case WeightedLicenseSeatEstimateStatus.Rejected:
                switch (next)
                {
                    case WeightedLicenseSeatEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedLicenseSeatEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted license seat estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted license seat estimate"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity - (double)UpdatedAt, 2);
    }

    public WeightedLicenseSeatEstimate Copy()
    {
        return new WeightedLicenseSeatEstimate(
            Code,
            Name,
            Capacity,
            ContactEmail,
            UpdatedAt,
            SalaryBand,
            Timezone,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
