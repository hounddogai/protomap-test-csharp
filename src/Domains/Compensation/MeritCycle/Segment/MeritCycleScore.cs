namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle score is in its life cycle.</summary>
public enum MeritCycleScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One merit cycle score, with what the planners need to act on it.</summary>
public sealed class MeritCycleScore(string code, string name, double margin, int updatedAt, string jobCode, MeritCycleScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int UpdatedAt { get; set; } = updatedAt;
    public string JobCode { get; set; } = jobCode;
    public MeritCycleScoreStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle score in status requested.</summary>
    public static MeritCycleScore NewMeritCycleScore(string code, string name)
    {
        return new MeritCycleScore(code, name, 0.0, 0, "", MeritCycleScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleScoreStatus next)
    {
        switch (Status)
        {
            case MeritCycleScoreStatus.Requested:
                return next == MeritCycleScoreStatus.Granted || next == MeritCycleScoreStatus.Revoked;
            case MeritCycleScoreStatus.Granted:
                switch (next)
                {
                    case MeritCycleScoreStatus.Revoked:
                        return true;
                }
                return false;
            case MeritCycleScoreStatus.Revoked:
                return next == MeritCycleScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(MeritCycleScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)UpdatedAt * Margin, 2);
    }

    public MeritCycleScore Copy()
    {
        return new MeritCycleScore(Code, Name, Margin, UpdatedAt, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
