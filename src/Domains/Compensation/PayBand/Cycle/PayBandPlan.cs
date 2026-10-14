namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band plan is in its life cycle.</summary>
public enum PayBandPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pay band plan, with what the planners need to act on it.</summary>
public sealed class PayBandPlan(string code, string label, int leaveDays, int attempts, string checksum, string timezone, string skill, int batchSize, string location, bool archived, PayBandPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public int Attempts { get; set; } = attempts;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public PayBandPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pay band plan in status requested.</summary>
    public static PayBandPlan NewPayBandPlan(string code, string label)
    {
        return new PayBandPlan(code, label, 0, 0, "", "", "", 0, "", false, PayBandPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandPlanStatus next)
    {
        switch (Status)
        {
            case PayBandPlanStatus.Requested:
                return next == PayBandPlanStatus.Granted || next == PayBandPlanStatus.Expired;
            case PayBandPlanStatus.Granted:
                switch (next)
                {
                    case PayBandPlanStatus.Expired:
                        return true;
                }
                return false;
            case PayBandPlanStatus.Revoked:
                return next == PayBandPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayBandPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay band plan"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)BatchSize + (double)LeaveDays, 2);
    }

    public PayBandPlan Copy()
    {
        return new PayBandPlan(
            Code,
            Label,
            LeaveDays,
            Attempts,
            Checksum,
            Timezone,
            Skill,
            BatchSize,
            Location,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
