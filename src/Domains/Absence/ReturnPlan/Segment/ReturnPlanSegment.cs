namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan segment is in its life cycle.</summary>
public enum ReturnPlanSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A return plan segment as the return plan team keeps it.</summary>
public sealed class ReturnPlanSegment(string code, string name, int graceDays, int startsAt, string segment, double score, double fte, string contactEmail, double ratio, string summary, double discount, ReturnPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int StartsAt { get; set; } = startsAt;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public string Summary { get; set; } = summary;
    public double Discount { get; set; } = discount;
    public ReturnPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a return plan segment in status idle.</summary>
    public static ReturnPlanSegment NewReturnPlanSegment(string code, string name)
    {
        return new ReturnPlanSegment(code, name, 0, 0, "", 0.0, 0.0, "", 0.0, "", 0.0, ReturnPlanSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanSegmentStatus next)
    {
        switch (Status)
        {
            case ReturnPlanSegmentStatus.Idle:
                return next == ReturnPlanSegmentStatus.Draining || next == ReturnPlanSegmentStatus.Stopped;
            case ReturnPlanSegmentStatus.Warming:
                switch (next)
                {
                    case ReturnPlanSegmentStatus.Ready:
                        return true;
                }
                return false;
            case ReturnPlanSegmentStatus.Ready:
                switch (next)
                {
                    case ReturnPlanSegmentStatus.Stopped:
                        return true;
                }
                return false;
            case ReturnPlanSegmentStatus.Draining:
                return next == ReturnPlanSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReturnPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"return plan segment"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Discount + (double)StartsAt, 2);
    }

    public ReturnPlanSegment Copy()
    {
        return new ReturnPlanSegment(
            Code,
            Name,
            GraceDays,
            StartsAt,
            Segment,
            Score,
            Fte,
            ContactEmail,
            Ratio,
            Summary,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
