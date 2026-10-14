namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate preference is in its life cycle.</summary>
public enum WeightedRatePreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted rate preference and its current status.</summary>
public sealed class WeightedRatePreference(string code, string title, int leaveDays, bool active, int endsAt, string jobCode, string skill, WeightedRatePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public string JobCode { get; set; } = jobCode;
    public string Skill { get; set; } = skill;
    public WeightedRatePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate preference in status proposed.</summary>
    public static WeightedRatePreference NewWeightedRatePreference(string code, string title)
    {
        return new WeightedRatePreference(code, title, 0, false, 0, "", "", WeightedRatePreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRatePreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRatePreferenceStatus next)
    {
        switch (Status)
        {
            case WeightedRatePreferenceStatus.Proposed:
                switch (next)
                {
                    case WeightedRatePreferenceStatus.Rejected:
                    case WeightedRatePreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedRatePreferenceStatus.Approved:
                switch (next)
                {
                    case WeightedRatePreferenceStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedRatePreferenceStatus.Rejected:
                return next == WeightedRatePreferenceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedRatePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)LeaveDays - (double)EndsAt, 2);
    }

    public WeightedRatePreference Copy()
    {
        return new WeightedRatePreference(Code, Title, LeaveDays, Active, EndsAt, JobCode, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
