namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario proposal is in its life cycle.</summary>
public enum ScenarioProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scenario proposal, with what the planners need to act on it.</summary>
public sealed class ScenarioProposal(string code, string title, double ratio, int createdAt, string jobCode, ScenarioProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int CreatedAt { get; set; } = createdAt;
    public string JobCode { get; set; } = jobCode;
    public ScenarioProposalStatus Status { get; set; } = status;

    /// <summary>Starts a scenario proposal in status idle.</summary>
    public static ScenarioProposal NewScenarioProposal(string code, string title)
    {
        return new ScenarioProposal(code, title, 0.0, 0, "", ScenarioProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioProposalStatus next)
    {
        switch (Status)
        {
            case ScenarioProposalStatus.Idle:
                return next == ScenarioProposalStatus.Stopped;
            case ScenarioProposalStatus.Warming:
                switch (next)
                {
                    case ScenarioProposalStatus.Draining:
                    case ScenarioProposalStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioProposalStatus.Ready:
                return next == ScenarioProposalStatus.Draining || next == ScenarioProposalStatus.Stopped;
            case ScenarioProposalStatus.Draining:
                return next == ScenarioProposalStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScenarioProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)CreatedAt - Ratio, 2);
    }

    public ScenarioProposal Copy()
    {
        return new ScenarioProposal(Code, Title, Ratio, CreatedAt, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
