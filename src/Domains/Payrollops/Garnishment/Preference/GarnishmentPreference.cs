namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment preference is in its life cycle.</summary>
public enum GarnishmentPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A garnishment preference. Its code is unique within a tenant.</summary>
public sealed class GarnishmentPreference(string code, string title, int limit, string salaryBand, int windowDays, double threshold, string jobCode, GarnishmentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public int WindowDays { get; set; } = windowDays;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public GarnishmentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment preference in status draft.</summary>
    public static GarnishmentPreference NewGarnishmentPreference(string code, string title)
    {
        return new GarnishmentPreference(code, title, 0, "", 0, 0.0, "", GarnishmentPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentPreferenceStatus next)
    {
        switch (Status)
        {
            case GarnishmentPreferenceStatus.Draft:
                switch (next)
                {
                    case GarnishmentPreferenceStatus.Active:
                    case GarnishmentPreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case GarnishmentPreferenceStatus.Pending:
                return next == GarnishmentPreferenceStatus.Active || next == GarnishmentPreferenceStatus.Suspended;
            case GarnishmentPreferenceStatus.Active:
                switch (next)
                {
                    case GarnishmentPreferenceStatus.Suspended:
                    case GarnishmentPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentPreferenceStatus.Suspended:
                switch (next)
                {
                    case GarnishmentPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment preference"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)WindowDays + (double)Limit, 2);
    }

    public GarnishmentPreference Copy()
    {
        return new GarnishmentPreference(Code, Title, Limit, SalaryBand, WindowDays, Threshold, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
