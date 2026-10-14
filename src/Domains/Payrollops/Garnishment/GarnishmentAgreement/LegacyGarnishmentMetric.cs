namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy garnishment metric is in its life cycle.</summary>
public enum LegacyGarnishmentMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy garnishment metric and its current status.</summary>
public sealed class LegacyGarnishmentMetric(string code, string label, double rate, string skill, int durationMinutes, bool pinned, LegacyGarnishmentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Pinned { get; set; } = pinned;
    public LegacyGarnishmentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a legacy garnishment metric in status proposed.</summary>
    public static LegacyGarnishmentMetric NewLegacyGarnishmentMetric(string code, string label)
    {
        return new LegacyGarnishmentMetric(code, label, 0.0, "", 0, false, LegacyGarnishmentMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyGarnishmentMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyGarnishmentMetricStatus next)
    {
        switch (Status)
        {
            case LegacyGarnishmentMetricStatus.Proposed:
                return next == LegacyGarnishmentMetricStatus.Withdrawn;
            case LegacyGarnishmentMetricStatus.Approved:
                return next == LegacyGarnishmentMetricStatus.Rejected || next == LegacyGarnishmentMetricStatus.Withdrawn;
            case LegacyGarnishmentMetricStatus.Rejected:
                switch (next)
                {
                    case LegacyGarnishmentMetricStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyGarnishmentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy garnishment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)DurationMinutes - Rate, 2);
    }

    public LegacyGarnishmentMetric Copy()
    {
        return new LegacyGarnishmentMetric(Code, Label, Rate, Skill, DurationMinutes, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
