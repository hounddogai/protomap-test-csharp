namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted pay run preference is in its life cycle.</summary>
public enum WeightedPayRunPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted pay run preference. Its code is unique within a tenant.</summary>
public sealed class WeightedPayRunPreference(string code, string title, string source, string region, double score, string skill, double margin, int limit, int windowDays, double rate, WeightedPayRunPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public double Score { get; set; } = score;
    public string Skill { get; set; } = skill;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public int WindowDays { get; set; } = windowDays;
    public double Rate { get; set; } = rate;
    public WeightedPayRunPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted pay run preference in status proposed.</summary>
    public static WeightedPayRunPreference NewWeightedPayRunPreference(string code, string title)
    {
        return new WeightedPayRunPreference(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            0,
            0.0,
            WeightedPayRunPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayRunPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayRunPreferenceStatus next)
    {
        switch (Status)
        {
            case WeightedPayRunPreferenceStatus.Proposed:
                return next == WeightedPayRunPreferenceStatus.Approved || next == WeightedPayRunPreferenceStatus.Withdrawn;
            case WeightedPayRunPreferenceStatus.Approved:
                return next == WeightedPayRunPreferenceStatus.Rejected;
            case WeightedPayRunPreferenceStatus.Rejected:
                return next == WeightedPayRunPreferenceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedPayRunPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted pay run preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Limit + (double)WindowDays, 2);
    }

    public WeightedPayRunPreference Copy()
    {
        return new WeightedPayRunPreference(
            Code,
            Title,
            Source,
            Region,
            Score,
            Skill,
            Margin,
            Limit,
            WindowDays,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
