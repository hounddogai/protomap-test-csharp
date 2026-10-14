namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual merit cycle proposal is in its life cycle.</summary>
public enum ManualMeritCycleProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual merit cycle proposal as the merit cycle team keeps it.</summary>
public sealed class ManualMeritCycleProposal(string code, string label, double floorValue, int expiresAt, double hours, string country, ManualMeritCycleProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public string Country { get; set; } = country;
    public ManualMeritCycleProposalStatus Status { get; set; } = status;

    /// <summary>Starts a manual merit cycle proposal in status idle.</summary>
    public static ManualMeritCycleProposal NewManualMeritCycleProposal(string code, string label)
    {
        return new ManualMeritCycleProposal(code, label, 0.0, 0, 0.0, "", ManualMeritCycleProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualMeritCycleProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualMeritCycleProposalStatus next)
    {
        switch (Status)
        {
            case ManualMeritCycleProposalStatus.Idle:
                return next == ManualMeritCycleProposalStatus.Draining || next == ManualMeritCycleProposalStatus.Stopped;
            case ManualMeritCycleProposalStatus.Warming:
                switch (next)
                {
                    case ManualMeritCycleProposalStatus.Stopped:
                        return true;
                }
                return false;
            case ManualMeritCycleProposalStatus.Ready:
                return next == ManualMeritCycleProposalStatus.Draining || next == ManualMeritCycleProposalStatus.Stopped;
            case ManualMeritCycleProposalStatus.Draining:
                return next == ManualMeritCycleProposalStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualMeritCycleProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual merit cycle proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(FloorValue * Hours, 2);
    }

    public ManualMeritCycleProposal Copy()
    {
        return new ManualMeritCycleProposal(Code, Label, FloorValue, ExpiresAt, Hours, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
