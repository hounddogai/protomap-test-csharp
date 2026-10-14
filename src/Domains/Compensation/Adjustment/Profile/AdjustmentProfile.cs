namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment profile is in its life cycle.</summary>
public enum AdjustmentProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adjustment profile as the adjustment team keeps it.</summary>
public sealed class AdjustmentProfile(string code, string name, string team, double score, int graceDays, int limit, string label, double rate, int windowDays, string employeeId, AdjustmentProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public AdjustmentProfileStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment profile in status requested.</summary>
    public static AdjustmentProfile NewAdjustmentProfile(string code, string name)
    {
        return new AdjustmentProfile(code, name, "", 0.0, 0, 0, "", 0.0, 0, "", AdjustmentProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentProfileStatus next)
    {
        switch (Status)
        {
            case AdjustmentProfileStatus.Requested:
                switch (next)
                {
                    case AdjustmentProfileStatus.Expired:
                        return true;
                }
                return false;
            case AdjustmentProfileStatus.Granted:
                switch (next)
                {
                    case AdjustmentProfileStatus.Revoked:
                    case AdjustmentProfileStatus.Expired:
                        return true;
                }
                return false;
            case AdjustmentProfileStatus.Revoked:
                return next == AdjustmentProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdjustmentProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment profile"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays - Rate, 2);
    }

    public AdjustmentProfile Copy()
    {
        return new AdjustmentProfile(
            Code,
            Name,
            Team,
            Score,
            GraceDays,
            Limit,
            Label,
            Rate,
            WindowDays,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
