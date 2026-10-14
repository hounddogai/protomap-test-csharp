namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual bonus pool proposal is in its life cycle.</summary>
public enum ManualBonusPoolProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual bonus pool proposal as the bonus pool team keeps it.</summary>
public sealed class ManualBonusPoolProposal(string code, string name, string country, string badge, string grade, double rate, ManualBonusPoolProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public ManualBonusPoolProposalStatus Status { get; set; } = status;

    /// <summary>Starts a manual bonus pool proposal in status new.</summary>
    public static ManualBonusPoolProposal NewManualBonusPoolProposal(string code, string name)
    {
        return new ManualBonusPoolProposal(code, name, "", "", "", 0.0, ManualBonusPoolProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBonusPoolProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBonusPoolProposalStatus next)
    {
        switch (Status)
        {
            case ManualBonusPoolProposalStatus.New:
                switch (next)
                {
                    case ManualBonusPoolProposalStatus.Done:
                        return true;
                }
                return false;
            case ManualBonusPoolProposalStatus.Queued:
                return next == ManualBonusPoolProposalStatus.Running || next == ManualBonusPoolProposalStatus.Done;
            case ManualBonusPoolProposalStatus.Running:
                return next == ManualBonusPoolProposalStatus.Failed;
            case ManualBonusPoolProposalStatus.Done:
                return next == ManualBonusPoolProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualBonusPoolProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual bonus pool proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual bonus pool proposal"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public ManualBonusPoolProposal Copy()
    {
        return new ManualBonusPoolProposal(Code, Name, Country, Badge, Grade, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
