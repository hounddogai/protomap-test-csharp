namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment summary is in its life cycle.</summary>
public enum AdjustmentSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adjustment summary. Its code is unique within a tenant.</summary>
public sealed class AdjustmentSummary(string code, string label, double rate, string grade, int revision, int expiresAt, bool visible, AdjustmentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public int Revision { get; set; } = revision;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Visible { get; set; } = visible;
    public AdjustmentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment summary in status idle.</summary>
    public static AdjustmentSummary NewAdjustmentSummary(string code, string label)
    {
        return new AdjustmentSummary(code, label, 0.0, "", 0, 0, false, AdjustmentSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentSummaryStatus next)
    {
        switch (Status)
        {
            case AdjustmentSummaryStatus.Idle:
                switch (next)
                {
                    case AdjustmentSummaryStatus.Ready:
                    case AdjustmentSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case AdjustmentSummaryStatus.Warming:
                switch (next)
                {
                    case AdjustmentSummaryStatus.Draining:
                    case AdjustmentSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case AdjustmentSummaryStatus.Ready:
                return next == AdjustmentSummaryStatus.Stopped;
            case AdjustmentSummaryStatus.Draining:
                return next == AdjustmentSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdjustmentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt + (double)Revision, 2);
    }

    public AdjustmentSummary Copy()
    {
        return new AdjustmentSummary(Code, Label, Rate, Grade, Revision, ExpiresAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
