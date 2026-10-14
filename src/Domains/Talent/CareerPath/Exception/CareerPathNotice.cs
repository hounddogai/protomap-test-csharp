namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path notice is in its life cycle.</summary>
public enum CareerPathNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path notice. Its code is unique within a tenant.</summary>
public sealed class CareerPathNotice(string code, string label, double ceiling, double ratio, bool active, CareerPathNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public bool Active { get; set; } = active;
    public CareerPathNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a career path notice in status planned.</summary>
    public static CareerPathNotice NewCareerPathNotice(string code, string label)
    {
        return new CareerPathNotice(code, label, 0.0, 0.0, false, CareerPathNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathNoticeStatus next)
    {
        switch (Status)
        {
            case CareerPathNoticeStatus.Planned:
                switch (next)
                {
                    case CareerPathNoticeStatus.Scheduled:
                        return true;
                }
                return false;
            case CareerPathNoticeStatus.Scheduled:
                switch (next)
                {
                    case CareerPathNoticeStatus.InProgress:
                    case CareerPathNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathNoticeStatus.InProgress:
                return next == CareerPathNoticeStatus.Completed;
            case CareerPathNoticeStatus.Completed:
                return next == CareerPathNoticeStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path notice"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling + Ratio, 2);
    }

    public CareerPathNotice Copy()
    {
        return new CareerPathNotice(Code, Label, Ceiling, Ratio, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
