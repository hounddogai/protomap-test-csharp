namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model override is in its life cycle.</summary>
public enum CostModelOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A cost model override as the cost model team keeps it.</summary>
public sealed class CostModelOverride(string code, string title, int endsAt, int retries, string locale, double threshold, string department, double floorValue, string skill, int startsAt, CostModelOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public int StartsAt { get; set; } = startsAt;
    public CostModelOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a cost model override in status requested.</summary>
    public static CostModelOverride NewCostModelOverride(string code, string title)
    {
        return new CostModelOverride(code, title, 0, 0, "", 0.0, "", 0.0, "", 0, CostModelOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelOverrideStatus next)
    {
        switch (Status)
        {
            case CostModelOverrideStatus.Requested:
                switch (next)
                {
                    case CostModelOverrideStatus.Revoked:
                    case CostModelOverrideStatus.Expired:
                        return true;
                }
                return false;
            case CostModelOverrideStatus.Granted:
                return next == CostModelOverrideStatus.Revoked || next == CostModelOverrideStatus.Expired;
            case CostModelOverrideStatus.Revoked:
                return next == CostModelOverrideStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold - (double)Retries, 2);
    }

    public CostModelOverride Copy()
    {
        return new CostModelOverride(
            Code,
            Title,
            EndsAt,
            Retries,
            Locale,
            Threshold,
            Department,
            FloorValue,
            Skill,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
