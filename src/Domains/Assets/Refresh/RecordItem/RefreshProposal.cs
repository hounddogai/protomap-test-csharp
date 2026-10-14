namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh proposal is in its life cycle.</summary>
public enum RefreshProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a refresh proposal and its current status.</summary>
public sealed class RefreshProposal(string code, string title, int windowDays, int durationMinutes, double fte, int startsAt, string jobCode, int revision, string managerId, string costCenter, RefreshProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Fte { get; set; } = fte;
    public int StartsAt { get; set; } = startsAt;
    public string JobCode { get; set; } = jobCode;
    public int Revision { get; set; } = revision;
    public string ManagerId { get; set; } = managerId;
    public string CostCenter { get; set; } = costCenter;
    public RefreshProposalStatus Status { get; set; } = status;

    /// <summary>Starts a refresh proposal in status idle.</summary>
    public static RefreshProposal NewRefreshProposal(string code, string title)
    {
        return new RefreshProposal(code, title, 0, 0, 0.0, 0, "", 0, "", "", RefreshProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshProposalStatus next)
    {
        switch (Status)
        {
            case RefreshProposalStatus.Idle:
                switch (next)
                {
                    case RefreshProposalStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshProposalStatus.Warming:
                switch (next)
                {
                    case RefreshProposalStatus.Ready:
                        return true;
                }
                return false;
            case RefreshProposalStatus.Ready:
                switch (next)
                {
                    case RefreshProposalStatus.Draining:
                    case RefreshProposalStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshProposalStatus.Draining:
                return next == RefreshProposalStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RefreshProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte * (double)DurationMinutes, 2);
    }

    public RefreshProposal Copy()
    {
        return new RefreshProposal(
            Code,
            Title,
            WindowDays,
            DurationMinutes,
            Fte,
            StartsAt,
            JobCode,
            Revision,
            ManagerId,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
