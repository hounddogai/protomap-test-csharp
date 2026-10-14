namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient refresh profile is in its life cycle.</summary>
public enum LenientRefreshProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient refresh profile and its current status.</summary>
public sealed class LenientRefreshProfile(string code, string label, string checksum, string team, double margin, double weight, string phone, string reviewer, int durationMinutes, LenientRefreshProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Team { get; set; } = team;
    public double Margin { get; set; } = margin;
    public double Weight { get; set; } = weight;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LenientRefreshProfileStatus Status { get; set; } = status;

    /// <summary>Starts a lenient refresh profile in status idle.</summary>
    public static LenientRefreshProfile NewLenientRefreshProfile(string code, string label)
    {
        return new LenientRefreshProfile(code, label, "", "", 0.0, 0.0, "", "", 0, LenientRefreshProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRefreshProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRefreshProfileStatus next)
    {
        switch (Status)
        {
            case LenientRefreshProfileStatus.Idle:
                switch (next)
                {
                    case LenientRefreshProfileStatus.Warming:
                    case LenientRefreshProfileStatus.Draining:
                        return true;
                }
                return false;
            case LenientRefreshProfileStatus.Warming:
                return next == LenientRefreshProfileStatus.Ready || next == LenientRefreshProfileStatus.Stopped;
            case LenientRefreshProfileStatus.Ready:
                switch (next)
                {
                    case LenientRefreshProfileStatus.Stopped:
                        return true;
                }
                return false;
            case LenientRefreshProfileStatus.Draining:
                return next == LenientRefreshProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientRefreshProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient refresh profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient refresh profile"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Weight - (double)DurationMinutes, 2);
    }

    public LenientRefreshProfile Copy()
    {
        return new LenientRefreshProfile(
            Code,
            Label,
            Checksum,
            Team,
            Margin,
            Weight,
            Phone,
            Reviewer,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
