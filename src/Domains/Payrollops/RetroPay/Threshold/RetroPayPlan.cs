namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay plan is in its life cycle.</summary>
public enum RetroPayPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One retro pay plan, with what the planners need to act on it.</summary>
public sealed class RetroPayPlan(string code, string name, double cost, int sequence, int quantity, string team, int updatedAt, double ratio, int endsAt, string notes, RetroPayPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public int Quantity { get; set; } = quantity;
    public string Team { get; set; } = team;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public string Notes { get; set; } = notes;
    public RetroPayPlanStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay plan in status idle.</summary>
    public static RetroPayPlan NewRetroPayPlan(string code, string name)
    {
        return new RetroPayPlan(code, name, 0.0, 0, 0, "", 0, 0.0, 0, "", RetroPayPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayPlanStatus next)
    {
        switch (Status)
        {
            case RetroPayPlanStatus.Idle:
                return next == RetroPayPlanStatus.Warming || next == RetroPayPlanStatus.Ready;
            case RetroPayPlanStatus.Warming:
                return next == RetroPayPlanStatus.Draining || next == RetroPayPlanStatus.Stopped;
            case RetroPayPlanStatus.Ready:
                return next == RetroPayPlanStatus.Stopped;
            case RetroPayPlanStatus.Draining:
                return next == RetroPayPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RetroPayPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round(Cost - (double)Quantity, 2);
    }

    public RetroPayPlan Copy()
    {
        return new RetroPayPlan(Code, Name, Cost, Sequence, Quantity, Team, UpdatedAt, Ratio, EndsAt, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
