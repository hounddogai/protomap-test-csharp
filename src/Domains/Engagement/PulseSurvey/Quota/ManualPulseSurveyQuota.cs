namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey quota is in its life cycle.</summary>
public enum ManualPulseSurveyQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual pulse survey quota as the pulse survey team keeps it.</summary>
public sealed class ManualPulseSurveyQuota(string code, string name, string jobCode, double cost, bool approved, string region, double ceiling, int revision, string unit, ManualPulseSurveyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public string Region { get; set; } = region;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public ManualPulseSurveyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey quota in status planned.</summary>
    public static ManualPulseSurveyQuota NewManualPulseSurveyQuota(string code, string name)
    {
        return new ManualPulseSurveyQuota(
            code,
            name,
            "",
            0.0,
            false,
            "",
            0.0,
            0,
            "",
            ManualPulseSurveyQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveyQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveyQuotaStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveyQuotaStatus.Planned:
                switch (next)
                {
                    case ManualPulseSurveyQuotaStatus.Scheduled:
                        return true;
                }
                return false;
            case ManualPulseSurveyQuotaStatus.Scheduled:
                switch (next)
                {
                    case ManualPulseSurveyQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualPulseSurveyQuotaStatus.InProgress:
                switch (next)
                {
                    case ManualPulseSurveyQuotaStatus.Completed:
                    case ManualPulseSurveyQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualPulseSurveyQuotaStatus.Completed:
                switch (next)
                {
                    case ManualPulseSurveyQuotaStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost + (double)Revision, 2);
    }

    public ManualPulseSurveyQuota Copy()
    {
        return new ManualPulseSurveyQuota(
            Code,
            Name,
            JobCode,
            Cost,
            Approved,
            Region,
            Ceiling,
            Revision,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
