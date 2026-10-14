namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy retro pay plan is in its life cycle.</summary>
public enum LegacyRetroPayPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy retro pay plan, with what the planners need to act on it.</summary>
public sealed class LegacyRetroPayPlan(string code, string name, int retries, string source, int capacity, string title, string salaryBand, string country, string summary, LegacyRetroPayPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Source { get; set; } = source;
    public int Capacity { get; set; } = capacity;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Country { get; set; } = country;
    public string Summary { get; set; } = summary;
    public LegacyRetroPayPlanStatus Status { get; set; } = status;

    /// <summary>Starts a legacy retro pay plan in status idle.</summary>
    public static LegacyRetroPayPlan NewLegacyRetroPayPlan(string code, string name)
    {
        return new LegacyRetroPayPlan(code, name, 0, "", 0, "", "", "", "", LegacyRetroPayPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRetroPayPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRetroPayPlanStatus next)
    {
        switch (Status)
        {
            case LegacyRetroPayPlanStatus.Idle:
                switch (next)
                {
                    case LegacyRetroPayPlanStatus.Ready:
                        return true;
                }
                return false;
            case LegacyRetroPayPlanStatus.Warming:
                switch (next)
                {
                    case LegacyRetroPayPlanStatus.Ready:
                    case LegacyRetroPayPlanStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyRetroPayPlanStatus.Ready:
                switch (next)
                {
                    case LegacyRetroPayPlanStatus.Draining:
                    case LegacyRetroPayPlanStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyRetroPayPlanStatus.Draining:
                return next == LegacyRetroPayPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LegacyRetroPayPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy retro pay plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Retries * (double)Capacity, 2);
    }

    public LegacyRetroPayPlan Copy()
    {
        return new LegacyRetroPayPlan(
            Code,
            Name,
            Retries,
            Source,
            Capacity,
            Title,
            SalaryBand,
            Country,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
