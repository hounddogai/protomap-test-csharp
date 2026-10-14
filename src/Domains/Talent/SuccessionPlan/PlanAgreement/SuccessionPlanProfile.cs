namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan profile is in its life cycle.</summary>
public enum SuccessionPlanProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a succession plan profile and its current status.</summary>
public sealed class SuccessionPlanProfile(string code, string title, int capacity, string segment, int endsAt, SuccessionPlanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public SuccessionPlanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan profile in status idle.</summary>
    public static SuccessionPlanProfile NewSuccessionPlanProfile(string code, string title)
    {
        return new SuccessionPlanProfile(code, title, 0, "", 0, SuccessionPlanProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanProfileStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanProfileStatus.Idle:
                return next == SuccessionPlanProfileStatus.Stopped;
            case SuccessionPlanProfileStatus.Warming:
                switch (next)
                {
                    case SuccessionPlanProfileStatus.Draining:
                        return true;
                }
                return false;
            case SuccessionPlanProfileStatus.Ready:
                return next == SuccessionPlanProfileStatus.Draining || next == SuccessionPlanProfileStatus.Stopped;
            case SuccessionPlanProfileStatus.Draining:
                return next == SuccessionPlanProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt * (double)Capacity, 2);
    }

    public SuccessionPlanProfile Copy()
    {
        return new SuccessionPlanProfile(Code, Title, Capacity, Segment, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
