namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped scenario ledger is in its life cycle.</summary>
public enum CappedScenarioLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A capped scenario ledger. Its code is unique within a tenant.</summary>
public sealed class CappedScenarioLedger(string code, string title, double margin, int revision, bool archived, string skill, double ceiling, double fte, string country, string channel, string summary, CappedScenarioLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public CappedScenarioLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a capped scenario ledger in status idle.</summary>
    public static CappedScenarioLedger NewCappedScenarioLedger(string code, string title)
    {
        return new CappedScenarioLedger(
            code,
            title,
            0.0,
            0,
            false,
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            CappedScenarioLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedScenarioLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedScenarioLedgerStatus next)
    {
        switch (Status)
        {
            case CappedScenarioLedgerStatus.Idle:
                return next == CappedScenarioLedgerStatus.Warming || next == CappedScenarioLedgerStatus.Ready;
            case CappedScenarioLedgerStatus.Warming:
                return next == CappedScenarioLedgerStatus.Ready || next == CappedScenarioLedgerStatus.Stopped;
            case CappedScenarioLedgerStatus.Ready:
                switch (next)
                {
                    case CappedScenarioLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case CappedScenarioLedgerStatus.Draining:
                return next == CappedScenarioLedgerStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedScenarioLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped scenario ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin * (double)Revision, 2);
    }

    public CappedScenarioLedger Copy()
    {
        return new CappedScenarioLedger(
            Code,
            Title,
            Margin,
            Revision,
            Archived,
            Skill,
            Ceiling,
            Fte,
            Country,
            Channel,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
