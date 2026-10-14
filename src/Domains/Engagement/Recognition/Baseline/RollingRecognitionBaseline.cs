namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling recognition baseline is in its life cycle.</summary>
public enum RollingRecognitionBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling recognition baseline as the recognition team keeps it.</summary>
public sealed class RollingRecognitionBaseline(string code, string title, bool visible, double minimum, string skill, int priority, int sequence, string department, int tenureMonths, double score, RollingRecognitionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Score { get; set; } = score;
    public RollingRecognitionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a rolling recognition baseline in status new.</summary>
    public static RollingRecognitionBaseline NewRollingRecognitionBaseline(string code, string title)
    {
        return new RollingRecognitionBaseline(
            code,
            title,
            false,
            0.0,
            "",
            0,
            0,
            "",
            0,
            0.0,
            RollingRecognitionBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRecognitionBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRecognitionBaselineStatus next)
    {
        switch (Status)
        {
            case RollingRecognitionBaselineStatus.New:
                return next == RollingRecognitionBaselineStatus.Running || next == RollingRecognitionBaselineStatus.Failed;
            case RollingRecognitionBaselineStatus.Queued:
                switch (next)
                {
                    case RollingRecognitionBaselineStatus.Running:
                    case RollingRecognitionBaselineStatus.Done:
                        return true;
                }
                return false;
            case RollingRecognitionBaselineStatus.Running:
                switch (next)
                {
                    case RollingRecognitionBaselineStatus.Done:
                        return true;
                }
                return false;
            case RollingRecognitionBaselineStatus.Done:
                return next == RollingRecognitionBaselineStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingRecognitionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling recognition baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Score - Minimum, 2);
    }

    public RollingRecognitionBaseline Copy()
    {
        return new RollingRecognitionBaseline(
            Code,
            Title,
            Visible,
            Minimum,
            Skill,
            Priority,
            Sequence,
            Department,
            TenureMonths,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
