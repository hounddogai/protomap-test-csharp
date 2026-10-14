namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey quota is in its life cycle.</summary>
public enum PulseSurveyQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pulse survey quota and its current status.</summary>
public sealed class PulseSurveyQuota(string code, string name, double amount, string timezone, string location, PulseSurveyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public string Location { get; set; } = location;
    public PulseSurveyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey quota in status proposed.</summary>
    public static PulseSurveyQuota NewPulseSurveyQuota(string code, string name)
    {
        return new PulseSurveyQuota(code, name, 0.0, "", "", PulseSurveyQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyQuotaStatus next)
    {
        switch (Status)
        {
            case PulseSurveyQuotaStatus.Proposed:
                return next == PulseSurveyQuotaStatus.Rejected || next == PulseSurveyQuotaStatus.Withdrawn;
            case PulseSurveyQuotaStatus.Approved:
                return next == PulseSurveyQuotaStatus.Withdrawn;
            case PulseSurveyQuotaStatus.Rejected:
                return next == PulseSurveyQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PulseSurveyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey quota"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PulseSurveyQuota Copy()
    {
        return new PulseSurveyQuota(Code, Name, Amount, Timezone, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
