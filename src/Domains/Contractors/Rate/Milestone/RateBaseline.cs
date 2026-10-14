namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate baseline is in its life cycle.</summary>
public enum RateBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rate baseline as the rate team keeps it.</summary>
public sealed class RateBaseline(string code, string name, string salaryBand, string locale, bool visible, int sequence, string region, RateBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public RateBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a rate baseline in status idle.</summary>
    public static RateBaseline NewRateBaseline(string code, string name)
    {
        return new RateBaseline(code, name, "", "", false, 0, "", RateBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateBaselineStatus next)
    {
        switch (Status)
        {
            case RateBaselineStatus.Idle:
                switch (next)
                {
                    case RateBaselineStatus.Draining:
                        return true;
                }
                return false;
            case RateBaselineStatus.Warming:
                return next == RateBaselineStatus.Draining || next == RateBaselineStatus.Stopped;
            case RateBaselineStatus.Ready:
                switch (next)
                {
                    case RateBaselineStatus.Draining:
                        return true;
                }
                return false;
            case RateBaselineStatus.Draining:
                switch (next)
                {
                    case RateBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public RateBaseline Copy()
    {
        return new RateBaseline(Code, Name, SalaryBand, Locale, Visible, Sequence, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
