namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment metric is in its life cycle.</summary>
public enum GarnishmentMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One garnishment metric, with what the planners need to act on it.</summary>
public sealed class GarnishmentMetric(string code, string name, int sequence, double ratio, string country, double floorValue, string locale, string skill, bool active, GarnishmentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public bool Active { get; set; } = active;
    public GarnishmentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment metric in status new.</summary>
    public static GarnishmentMetric NewGarnishmentMetric(string code, string name)
    {
        return new GarnishmentMetric(code, name, 0, 0.0, "", 0.0, "", "", false, GarnishmentMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentMetricStatus next)
    {
        switch (Status)
        {
            case GarnishmentMetricStatus.New:
                switch (next)
                {
                    case GarnishmentMetricStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentMetricStatus.Queued:
                return next == GarnishmentMetricStatus.Running || next == GarnishmentMetricStatus.Done;
            case GarnishmentMetricStatus.Running:
                switch (next)
                {
                    case GarnishmentMetricStatus.Done:
                        return true;
                }
                return false;
            case GarnishmentMetricStatus.Done:
                switch (next)
                {
                    case GarnishmentMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ratio - FloorValue, 2);
    }

    public GarnishmentMetric Copy()
    {
        return new GarnishmentMetric(
            Code,
            Name,
            Sequence,
            Ratio,
            Country,
            FloorValue,
            Locale,
            Skill,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
